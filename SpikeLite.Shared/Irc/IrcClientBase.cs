/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using SpikeLite.Communications.Irc;
using SpikeLite.Communications.Irc.Configuration;
using SpikeLite.Communications.Messaging;
using SpikeLite.Shared.Communications;
using Cadenza.Collections;

namespace SpikeLite.Irc
{
    public abstract class IrcClientBase : IIrcClient
    {
        #region Events
        public event EventHandler<PrivateMessageReceivedEventArgs> PrivateMessageReceived;
        public event EventHandler<PublicMessageReceivedEventArgs> PublicMessageReceived;
        #endregion

        #region Auto Properties
        public IPrivmsgParser MessageParser { get; set; }
        public ICommunicationManager CommunicationManager { get; protected set; }
        public Network Network { get; protected set; }
        public Server Server { get; protected set; }
        public bool SupportsIdentification { get; set; }
        #endregion

        #region OnEvent Methods
        protected virtual void OnPrivateMessageReceived(User user, string message)
        {
            EventHandler<PrivateMessageReceivedEventArgs> privateMessageReceived = this.PrivateMessageReceived;

            if (privateMessageReceived != null)
                privateMessageReceived(this, new PrivateMessageReceivedEventArgs(user, message));
        }

        protected virtual void OnPrivateMessageReceived(User user, string channelName, string message)
        {
            EventHandler<PublicMessageReceivedEventArgs> publicMessageReceived = this.PublicMessageReceived;

            if (publicMessageReceived != null)
                publicMessageReceived(this, new PublicMessageReceivedEventArgs(user, channelName, message));
        }
        #endregion

        protected virtual void JoinChannelsForNetwork()
        {
            Network.ServerList[0].ChannelList.ForEach(channel =>
            {
                JoinChannel(channel.Name);
            });
        }

        protected virtual bool NoticeIsExpectedServicesAgentMessage(string nickname, string notice)
        {
            if (nickname.Equals("nickserv", StringComparison.OrdinalIgnoreCase))
            {
                return notice.StartsWith("You are now identified for", StringComparison.OrdinalIgnoreCase) ||
                       (notice.StartsWith("The nickname", StringComparison.OrdinalIgnoreCase) && notice.EndsWith("is not registered", StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

        public abstract bool IsConnected { get; }

        public abstract void Connect(ICommunicationManager communicationManager, Network network);
        public abstract void DoAction(string channelName, string emoteText);
        public abstract void JoinChannel(string channelName);
        public abstract void PartChannel(string channelName);
        public abstract void Quit();
        public abstract void Quit(string message);
        public abstract void SendResponse(Communications.Response response);
    }
}