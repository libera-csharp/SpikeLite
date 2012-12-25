/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Linq;
using Cadenza.Collections;
using SpikeLite.AccessControl;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Modules.Admin
{
    /// <summary>
    /// This class allows us to manage "users" or authentication tokens within the bot's authentication repository.
    /// </summary>
    public class UserManagementModule : ModuleBase
    {
        #region Data Members and Properties 

        /// <summary>
        /// Gets or sets a <see cref="IrcAuthenticationModule"/> we can use for adding or removing known hosts.
        /// </summary>
        public IrcAuthenticationModule AuthenticationManager { get; set; }

        #endregion 

        #region Parsing

        public override void HandleRequest(Request request)
        {
            var message = request.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // The user must be root, the request must be a 1:1 PRIVMSG and it's gotta be of a known form factor.
            if (request.RequestFrom.AccessLevel >= AccessLevel.Root
                && request.RequestType == RequestType.Private
                && message[0].Equals("~users", StringComparison.OrdinalIgnoreCase)
                && (message.Length >= 2 && message.Length <= 5))
            {
                // users list
                if (message[1].Equals("list", StringComparison.OrdinalIgnoreCase) && message.Length == 2)
                {
                    ListUsers(request);
                }

                // users add hostmask level
                if (message[1].Equals("add", StringComparison.OrdinalIgnoreCase) && message.Length == 4)
                {
                    AddUser(message[2], message[3], request);
                }

                // users del hostmask
                if (message[1].Equals("del", StringComparison.OrdinalIgnoreCase) && message.Length == 3)
                {
                    DeleteUser(message[2], request);    
                }

                // TODO: Kog 1/13/2010 - We can probably replace this with something like a nested collection of funcs. Make it funky now.

                // users metadata ..
                if (message[1].Equals("metadata", StringComparison.OrdinalIgnoreCase) && message.Length == 5)
                {
                   // If they're trying to del the tag, do so.
                   if (message[2].Equals("del", StringComparison.OrdinalIgnoreCase))
                   {
                       DeleteMetaDatumForUser(message[3], message[4], request);
                   }
                   else
                   {
                       // if they give a key and a value, create or update the tag accordingly.
                       CreateOrUpdateMetaDatumForUser(message[2], message[3], message[4], request);
                   }
                }
            }
        }

        #endregion 

        #region User Operations

        /// <summary>
        /// Pretty prints all the users in our system.
        /// </summary>
        /// 
        /// <param name="request">A <see cref="Request"/> object we can use for creating a response to.</param>
        /// 
        /// <remarks>
        /// This may get to be incredibly spammy if you have a number of hosts.
        /// </remarks>
        private void ListUsers(Request request)
        {
            SendResponse(AuthenticationManager.GetHosts().Select(x => TranslateHostToString(x)).Implode(", "), request);
        }

        /// <summary>
        /// Attempts to add the given hostmask, if it doesn't already exist.
        /// </summary>
        /// 
        /// <param name="hostmask">The hostmask to add.</param>
        /// <param name="level">The level to add: either <code>ADMIN</code> or <code>PUBLIC</code>. </param>
        /// <param name="request">Our incoming <see cref="Request"/> object, which we use for creating our response.</param>
        /// 
        /// <remarks>
        /// At present we only add <see cref="HostMatchType.Start"/> type users, and if a proper level is not specified, it is assumed that the user wants
        /// a <see cref="AccessLevel.Public"/>. There is also currently no way to add a <see cref="AccessLevel.Root"/> user, which is by design.
        /// </remarks>
        private void AddUser(string hostmask, string level, Request request)
        {
            if (AuthenticationManager.KnowsHost(hostmask))
            {
                SendResponse(String.Format("The hostmask {0} already has an entry.", hostmask), request);
            }
            else
            {
                var host = new KnownHost
                {
                    AccessLevel = GetAccessLevelByName(level),
                    HostMask = hostmask,
                    HostMatchType = HostMatchType.Start,
                };
 
                AuthenticationManager.RememberHost(host);
                ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Private, String.Format("Learned host {0}", TranslateHostToString(host))));
            }
        }

        /// <summary>
        /// Attempts to delete a given hostmask, assuming it already exists.
        /// </summary>
        /// 
        /// <param name="hostmask">The hostmask to remove.</param>
        /// <param name="request">Our incoming <see cref="Request"/> object, which we use for creating our response.</param>
        private void DeleteUser(string hostmask, Request request)
        {
            if (!AuthenticationManager.KnowsHost(hostmask))
            {
                SendResponse(String.Format("The hostmask {0} already has no current entry.", hostmask), request);    
            }
            else
            {
                AuthenticationManager.ForgetHost(hostmask);
                SendResponse(String.Format("The hostmask {0} has been removed.", hostmask), request);
            }
        }

        #region Metadata Handling

        // TODO: Kog 1/13/2010 - The thought occurs to me that it would be really nice to have some sort of syntax allowing metadata to be added
        // TODO: Kog 1/13/2010 - or tags to be added with spaces. For now, let's just go with the crude stuff here.

        /// <summary>
        /// Deletes a metadatum for a given "tag" on a known host.
        /// </summary>
        /// 
        /// <param name="host">The literal string of a cloak to modify metadata for.</param>
        /// <param name="tag">The metadatum's "tag" (think of this like a key in a key/value pair). Must have no spaces</param>
        /// <param name="request">Our incoming <see cref="Request"/> object, which we use for creating our response.</param>
        /// 
        /// <remarks>
        /// If the host is not found (via a literal search), or the tag does not exist on the host, the outbound request will notify the calling user. 
        /// </remarks>
        private void DeleteMetaDatumForUser(string host, string tag, Request request)
        {
            var knownHost = AuthenticationManager.FindHostByCloak(host);
            var response = String.Format("Tag {0} successfully removed from {1}.", tag, host);

            // Make sure we have the host...
            if (null != knownHost)
            {
                var metaDatum = knownHost.MetaData.FirstOrDefault(x => x.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase));

                // Make sure we have that tag
                if (null != metaDatum)
                {
                    // TODO: Kog 1/13/2010 - perhaps this should just become one operation on the auth manager. The whole known host bit needs to be refactored most likely anyway.

                    // Tell our persistence layer to do the needful.
                    knownHost.MetaData.Remove(metaDatum);
                    AuthenticationManager.UpdateHost(knownHost);
                }
                else
                {
                    response = String.Format("Could not remove tag {0} from host {1}, no such tag known.", tag, host);    
                }
            }
            else
            {
                response = String.Format("Host {0} doesn't exist, can't remove tag {1}", host, tag);
            }
            
            SendResponse(response, request);
        }

        /// <summary>
        /// Attempts to either create a new metadatum entry on a known host, or update the value of a currently existing tag. 
        /// </summary>
        /// 
        /// <param name="host">The literal string of a cloak to modify metadata for.</param>
        /// <param name="tag">The metadatum's "tag" (think of this like a key in a key/value pair). Must have no spaces.</param>
        /// <param name="value">The metadatum's "value" (think of this like a value in a key/value pair). Must have no spaces.</param>
        /// <param name="request">Our incoming <see cref="Request"/> object, which we use for creating our response.</param>
        /// 
        /// <remarks>
        /// If the host is not found (via a literal search) the outbound request will notify the calling user.  
        /// </remarks>
        private void CreateOrUpdateMetaDatumForUser(string host, string tag, string value, Request request)
        {
            var knownHost = AuthenticationManager.FindHostByCloak(host);
            var response = String.Format("Tag {0} successfully added to {1}.", tag, host);

            // Make sure we have the host...
            if (null != knownHost)
            {
                // Attempt to find or create our tag.
                var metaDatum = knownHost.MetaData.FirstOrDefault(x => x.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase));

                if (null == metaDatum)
                {
                    // Create our new tag and add it to our mapped collection.
                    metaDatum = new KnownHostMetaDatum {Host = knownHost, Tag = tag};
                    knownHost.MetaData.Add(metaDatum);
                }

                // Slap in our new value.
                metaDatum.Value = value;

                // Tell our persistence layer to do the needful.
                AuthenticationManager.UpdateHost(knownHost);
            }
            else
            {
                response = String.Format("Host {0} doesn't exist, can't remove tag {1}", host, tag);
            }

            SendResponse(response, request);
        }

        #endregion

        #endregion

        #region Helper Methods

        /// <summary>
        /// A conveninence method to cut down on the repetitive chatter of sending a response through our module subsystem.
        /// </summary>
        /// 
        /// <param name="message">The message to send back out.</param>
        /// <param name="request">Our incoming <see cref="Request"/> object, which we use for creating our response.</param>
        public void SendResponse(string message, Request request)
        {
            ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Private, message));   
        }

        /// <summary>
        /// A pretty-printer for a <see cref="KnownHost"/>. 
        /// </summary>
        /// 
        /// <param name="host">A <see cref="KnownHost"/> to pretty-print.</param>
        /// 
        /// <returns>A pretty-printed <see cref="KnownHost"/> entry. Oh so pretty, oh so pretty.</returns>
        /// 
        /// <remarks>
        /// This may eventually become the ToString method on <see cref="KnownHost"/>, but for now this module is the only one that cares.
        /// </remarks>
        private static string TranslateHostToString(KnownHost host)
        {
            return String.Format("[{0}: level {1}, type {2}, [metadata: {3}]]", 
                                 host.HostMask, host.AccessLevel, host.HostMatchType, host.MetaData.Implode(", ", 
                                                                                                            x => String.Format("[{0} - {1}]", x.Tag, x.Value)));    
        }

        /// <summary>
        /// Does what it says on the tin - translates the name of an access level into an actual level.
        /// </summary>
        /// 
        /// <param name="levelName">The access level to use.</param>
        /// 
        /// <returns>The corresponding <see cref="AccessLevel"/> if the user hands us <code>ADMIN</code> or <code>PUBLIC</code>, else <code>PUBLIC</code>.</returns>
        private static AccessLevel GetAccessLevelByName(string levelName)
        {
            return levelName.Equals("admin", StringComparison.OrdinalIgnoreCase) ? AccessLevel.Admin : AccessLevel.Public;
        }

        #endregion 
    }
}
