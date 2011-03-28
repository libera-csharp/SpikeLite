/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using Sharkbite.Irc;

namespace SpikeLite.Communications.Messaging
{
    /// <summary>
    /// Provides a mechanism for translating our underlying IRC library's events and messages into something consistent the bot can use. This
    /// interface also allows for the creation of proxies.
    /// </summary>
    public interface IPrivmsgParser
    {
        /// <summary>
        /// Attempts to parse a 1:N PRIVMSG the bot has received. This would be when someone sends a message to a channel.
        /// </summary>
        /// 
        /// <param name="user">A <see cref="UserInfo"/> class containing information about the user sending the message.</param>
        /// <param name="channel">The name of the target channel of the message.</param>
        /// <param name="message">The message being sent.</param>
        void HandleMultiTargetMessage(UserInfo user, string channel, string message);

        /// <summary>
        /// Attempts to parse a 1:1 PRIVMSG (commonly known as a PM) the bot has received.
        /// </summary>
        /// 
        /// <param name="user">A <see cref="UserInfo"/> class containing information about the user sending the message.</param>
        /// <param name="message">The message being sent.</param>
        void HandleSingleTargetMessage(UserInfo user, string message);
    }
}
