/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Modules.Admin
{
    /// <summary>
    /// This administrative module allows users with <see cref="AccessLevel.Root"/> to shut down the bot with an optional quit message, as opposed to being required to
    /// send SIGTERM from the console.
    /// </summary>
    public class ShutdownModule : ModuleBase
    {
        /// <summary>
        /// Holds a reference to bot engine harness. We use this for sending termination events.
        /// </summary>
        public SpikeLite BotContext { get; set; }

        /// <summary>
        /// Checks to see if the user has called for a shutdown, and if so what their access level is. If they have the proper permissions we then attempt to parse an 
        /// optional quit message.
        /// </summary>
        /// 
        /// <param name="request">Our incoming message.</param>
        public override void HandleRequest(Request request)
        {
            // The user has permission and is indeed requesting a shutdown.
            if (request.RequestFrom.AccessLevel >= AccessLevel.Root && request.Message.StartsWith("~shutdown", StringComparison.OrdinalIgnoreCase))
            {
                // Parse off our quit message.
                string message = "No quit message specified.";
                int offset = request.Message.IndexOf("~shutdown") + 9;

                if (request.Message.Length > offset)
                {
                    message = request.Message.Substring(offset).Trim();    
                }

                BotContext.Shutdown(message);
            }
        }
    }
}
