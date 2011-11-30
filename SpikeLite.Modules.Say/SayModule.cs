/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;

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
        /// Handles responding to a user IFF they have proper access, and the trigger is ~name.
        /// </summary>
        /// 
        /// <param name="request">A <see cref="Request"/> representing a message sent to the bot.</param>
        public override void HandleRequest(Request request)
        {
            string[] messageArray = request.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (request.RequestFrom.AccessLevel >= AccessLevel.Public
                && request.RequestType == RequestType.Public
                && messageArray[0].Equals("~"+Name, StringComparison.OrdinalIgnoreCase))
            {
                ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Public, Phrase));
            }
        }
    }
}
