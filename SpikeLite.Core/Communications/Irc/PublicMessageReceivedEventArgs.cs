/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;

namespace SpikeLite.Communications.Irc
{
    public class PublicMessageReceivedEventArgs : EventArgs
    {
        public User User { get; private set; }
        public string ChannelName { get; private set; }
        public string Message { get; private set; }

        public PublicMessageReceivedEventArgs(User user, string channelName, string message)
        {
            User = user;
            ChannelName = channelName;
            Message = message;
        }
    }
}