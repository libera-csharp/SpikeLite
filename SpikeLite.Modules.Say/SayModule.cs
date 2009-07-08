/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using SpikeLite.Communications;
using SpikeLite.Persistence.Authentication;

namespace SpikeLite.Modules.Say
{
    /// <summary>
    /// A module prototype for a static "say" module that says a pre-canned message to a given user.
    /// </summary>
    public class SayModule : ModuleBase
    {
        /// <summary>
        /// Holds the phrase to say to the person in question.
        /// </summary>
        public string Phrase { get; set; }

        /// <summary>
        /// Handles responding to a user IFF they have proper access, the trigger is "~say" and we know the phrase to say.
        /// </summary>
        /// 
        /// <param name="request">A <see cref="Request"/> representing a message sent to the bot.</param>
        public override void HandleRequest(Request request)
        {
            string[] messageArray = request.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (request.RequestFrom.AccessLevel >= AccessLevel.Public
                && request.RequestType == RequestType.Public
                && messageArray[0].Equals("~say", StringComparison.OrdinalIgnoreCase) 
                && messageArray[1].Equals(Name, StringComparison.OrdinalIgnoreCase))
            {
                SendResponse(request);
            }
        }

        /// <summary>
        /// Sends the actual response to the user.
        /// </summary>
        /// 
        /// <param name="request">A <see cref="Request"/> representing a message sent to the bot.</param>
        private void SendResponse(Request request)
        {
            ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Public, Phrase));
        }
    }
}
