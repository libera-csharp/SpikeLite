/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.ServiceModel;
using SpikeLite.Communications;
using Spring.Context.Support;

namespace SpikeLite.IPC.WebHost.Services
{
    /// <summary>
    /// Provides us with a mechanism for messaging targets via the bot, from an external WS endpoint.
    /// </summary>
    [ServiceContract(Namespace = "http://tempuri.org")]
    public interface IMessagingService
    {
        /// <summary>
        /// Attempts to send a message to a channel. If a nick is supplied, the bot will attempt to address it, else it will address the entire channel.
        /// </summary>
        /// 
        /// <param name="channelTarget">The channel to target. If the bot is not in the channel then the message will not be sent.</param>
        /// <param name="messageText">The text of the message to send.</param>
        [OperationContract]
        [SecuredOperation]
        void SendMessage(string channelTarget, string messageText);    
    }

    /// <summary>
    /// Implements the <see cref="IMessagingService"/> contract, providing a concrete implementation of our service.
    /// </summary>
    public class MessagingService : IMessagingService
    {
        public void SendMessage(string channelTarget, string messageText)
        {
            // Get our application context from Spring.NET.
            var ctx = ContextRegistry.GetContext();

            // Grab our bean and spin it up.
            var mgr = (ctx.GetObject("CommunicationManager") as CommunicationManager);
            mgr.SendResponse(new Response { Channel = channelTarget, Message = messageText, ResponseType = ResponseType.Public });
        }
    }
}
