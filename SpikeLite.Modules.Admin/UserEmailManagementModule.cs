/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using SpikeLite.AccessControl;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Modules.Admin
{
    /// <summary>
    /// This module allows users to manage the email address that's associated with their credentials. While the bot doesn't really email anyone anything yet,
    /// this is useful for delegated auth.
    /// </summary>
    public class UserEmailManagementModule : ModuleBase
    {
        /// <summary>
        /// Gets or sets a <see cref="IrcAuthenticationModule"/> we can use for operating with accounts.
        /// </summary>
        public IrcAuthenticationModule AuthenticationManager { get; set; }

        public override void HandleRequest(Request request)
        {
            if (request.RequestFrom.AccessLevel >= AccessLevel.Public &&
                request.Message.StartsWith("~email", StringComparison.OrdinalIgnoreCase))
            {
                var splitMessage = request.Message.Split(' ');

                // We've got an operation.
                if (splitMessage.Length > 1)
                {
                    // Look up the user who sent the message.
                    var userToken = AuthenticationManager.ExchangeTokenForConcreteType<IrcUserToken>(request.RequestFrom.User);
                    var user = AuthenticationManager.FindHostByCloak(userToken.HostMask);

                    // Handle the case when the user wants to set their email.
                    if (splitMessage[1].Equals("set", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Make sure the user has given us an email.
                        if (splitMessage.Length > 2)
                        {
                            var email = splitMessage[2];

                            try
                            {
                                user.EmailAddress = email;
                                AuthenticationManager.UpdateHost(user);

                                Reply(request, ResponseTargetType.Private, "Your email has been set to {0}", email);
                            }
                            catch (Exception ex)
                            {
                                // Ruh roh, they've violated our unique constraint.
                                Logger.Warn("Caught an exception trying to set someone's email.", ex);
                                Reply(request, ResponseTargetType.Private, "Email {0} is already in use.", email);
                            }

                        }
                        else
                        {
                            // User hasn't given us an email, barf.
                            Reply(request, ResponseTargetType.Private, "No email specified, cannot set email address.");
                        }
                    }

                    // Handle the case when the user wants to check their email.
                    if (splitMessage[1].Equals("display", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseTargetType.Private,
                            String.IsNullOrEmpty(user.EmailAddress) ? "You have no email address set." : String.Format("Your currently set email is {0}.", user.EmailAddress)));
                    }
                }
                else
                {
                    // User has given us an invalid request, barf.
                    Reply(request, ResponseTargetType.Private, "Invalid request, please read the help for email.");
                }
            }
        }
    }
}