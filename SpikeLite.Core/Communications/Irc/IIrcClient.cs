/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using SpikeLite.Communications.Irc.Configuration;

namespace SpikeLite.Communications.Irc
{
    public interface IIrcClient
    {
        event EventHandler<PrivateMessageReceivedEventArgs> PrivateMessageReceived;
        event EventHandler<PublicMessageReceivedEventArgs> PublicMessageReceived;

        string Description { get; }
        bool IsConnected { get; }
        bool SupportsIdentification { get; set; }

        void Connect(Network network);
        void DoAction(string channelName, string emoteText);
        void JoinChannel(string channelName);
        void PartChannel(string channelName);
        void Quit();
        void Quit(string message);
        void SendResponse(Response response);
    }
}