/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
namespace SpikeLite.Communications
{
    /// <summary>
    /// This enum represents the types of response possible, in this case either a 1:1 or 1:N message.
    /// </summary>
    public enum ResponseType
    { 
        /// <summary>
        /// This is a 1:N message, being targeted at a channel.
        /// </summary>
        Public,

        /// <summary>
        /// This is a 1:1 message, being targeted at a single user.
        /// </summary>
        Private
    }

    /// <summary>
    /// This struct contains information about a message being sent by the bot.
    /// </summary>
    public struct Response
    {
        /// <summary>
        /// Gets or sets information about the channel to which this message might be heading.
        /// </summary>
        /// 
        /// <remarks>
        /// By convention this is the channel that sent the original request.
        /// </remarks>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets the nick of the user to respond to.
        /// </summary>
        public string Nick { get; set; }

        /// <summary>
        /// Gets or sets the type of response for this message. See the <see cref="ResponseType"/> type for details.
        /// </summary>
        public ResponseType ResponseType { get; set; }

        /// <summary>
        /// Gets or sets the actual message payload for the response.
        /// </summary>
        public string Message { get; set; }
    }
}