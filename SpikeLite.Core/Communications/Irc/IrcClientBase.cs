/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Reflection;
using Cadenza.Collections;
using SpikeLite.Communications.Irc.Configuration;

namespace SpikeLite.Communications.Irc
{
    public abstract class IrcClientBase : IIrcClient
    {
        #region Events
        public event EventHandler<PrivateMessageReceivedEventArgs> PrivateMessageReceived;
        public event EventHandler<PublicMessageReceivedEventArgs> PublicMessageReceived;
        public event EventHandler<PrivateMessageReceivedEventArgs> PrivateActionReceived;
        public event EventHandler<PublicMessageReceivedEventArgs> PublicActionReceived;
        #endregion

        #region Auto Properties
        public abstract bool IsConnected { get; }
        public Network Network { get; protected set; }
        public Server Server { get; protected set; }
        public bool SupportsIdentification { get; set; }
        public string Description { get; protected set; }
        #endregion

        #region Constructors
        protected IrcClientBase() { }
        protected IrcClientBase(Assembly assemblyForDescription)
        {
            var assemblyName = assemblyForDescription.GetName();

            Description = string.Format("{0} : {1}", assemblyName.Name, assemblyName.Version);
        }
        #endregion

        #region OnEvent Methods
        protected virtual void OnPrivateMessageReceived(User user, string message)
        {
            var privateMessageReceived = PrivateMessageReceived;

            if (privateMessageReceived != null)
                privateMessageReceived(this, new PrivateMessageReceivedEventArgs(user, message));
        }

        protected virtual void OnPublicMessageReceived(User user, string channelName, string message)
        {
            var publicMessageReceived = PublicMessageReceived;

            if (publicMessageReceived != null)
                publicMessageReceived(this, new PublicMessageReceivedEventArgs(user, channelName, message));
        }

        protected virtual void OnPrivateActionReceived(User user, string Action)
        {
            var privateActionReceived = PrivateActionReceived;

            if (privateActionReceived != null)
                privateActionReceived(this, new PrivateMessageReceivedEventArgs(user, Action));
        }

        protected virtual void OnPublicActionReceived(User user, string channelName, string Action)
        {
            var publicActionReceived = PublicActionReceived;

            if (publicActionReceived != null)
                publicActionReceived(this, new PublicMessageReceivedEventArgs(user, channelName, Action));
        }
        #endregion

        protected virtual void JoinChannelsForNetwork()
        {
            Network.ServerList[0].ChannelList.ForEach(channel => JoinChannel(channel.Name));
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

        public abstract void Connect(Network network);
        public abstract void DoAction(string channelName, string emoteText);
        public abstract void JoinChannel(string channelName);
        public abstract void PartChannel(string channelName);
        public abstract void Quit();
        public abstract void Quit(string message);
        public abstract void SendResponse(Response response);
    }
}