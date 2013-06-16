/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Linq;
using System.Threading;
using IrcDotNet;
using IrcDotNet.Ctcp;
using log4net.Ext.Trace;
using SpikeLite.Communications;
using SpikeLite.Communications.Irc;
using SpikeLite.Communications.Irc.Configuration;

namespace SpikeLite.Irc.IrcDotNet
{
    public class IrcClient : IrcClientBase
    {
        #region Data members
        private global::IrcDotNet.IrcClient _ircClient;
        private CtcpClient _ctcpClient;
        private readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(IrcClient));
        private int _nickRetryCount;
        private bool _userInitiatedDisconnect;
        #endregion

        public override bool IsConnected { get { return _ircClient != null && _ircClient.IsConnected; } }

        public IrcClient() : base(typeof(global::IrcDotNet.IrcClient).Assembly) { }

        public override void Connect(Network network)
        {
            Network = network;
            Server = Network.ServerList.First();

            Connect();
        }

        private void Connect()
        {
            _userInitiatedDisconnect = false;

            _ircClient = new global::IrcDotNet.IrcClient();
            _ctcpClient = new CtcpClient(_ircClient);

            _ircClient.Registered += ClientRegisteredWithIrcd;
            _ircClient.RawMessageReceived += IrcClient_RawMessageReceived;
            _ircClient.RawMessageSent += IrcClient_RawMessageSent;

            _ircClient.Connect(Server.Host, Server.Port ?? 6667, false, new IrcUserRegistrationInfo
            {
                NickName = Network.BotNickname,
                UserName = Network.BotUsername,
                RealName = Network.BotRealname
            });

            _ircClient.Disconnected += Disconnected;
        }

        public override void DoAction(string channelName, string emoteText)
        {
            _ctcpClient.SendAction(_ircClient.Channels.Single(c => c.Name.Equals(channelName, StringComparison.OrdinalIgnoreCase)), emoteText);
        }

        public override void JoinChannel(string channelName)
        {
            _ircClient.Channels.Join(channelName);
        }

        public override void PartChannel(string channelName)
        {
            _ircClient.Channels.Leave(channelName);
        }

        public override void Quit()
        {
            if (IsConnected)
            {
                _userInitiatedDisconnect = true;
                _logger.TraceFormat("Quitting with no message.");
                _ircClient.Quit();
            }
        }

        public override void Quit(string message)
        {
            if (IsConnected)
            {
                _userInitiatedDisconnect = true;
                _logger.TraceFormat("Sending quit message {0}.", message);
                _ircClient.Quit(message);
            }
        }

        public override void SendResponse(Response response)
        {
            try
            {
                if (response.ResponseType == ResponseType.Action)
                {
                    IIrcMessageTarget target = null;

                    if (response.ResponseTargetType == ResponseTargetType.Public)
                    {
                        target = _ircClient.Channels.FirstOrDefault(c => c.Name.Equals(response.Channel, StringComparison.OrdinalIgnoreCase));
                    }
                    else if (response.ResponseTargetType == ResponseTargetType.Private)
                    {
                        target = _ircClient.Users.FirstOrDefault(u => u.NickName.Equals(response.Nick, StringComparison.OrdinalIgnoreCase));
                    }

                    if (target != null)
                    {
                        _ctcpClient.SendAction(target, response.Message);
                    }
                }
                else if (response.ResponseType == ResponseType.Message)
                {
                    string target = null;

                    if (response.ResponseTargetType == ResponseTargetType.Public)
                    {
                        target = response.Channel;
                    }
                    else if (response.ResponseTargetType == ResponseTargetType.Private)
                    {
                        target = response.Nick;
                    }

                    if (target != null)
                    {
                        _ircClient.LocalUser.SendMessage(target, response.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.WarnFormat("Caught an exception trying to SendResponse on [channel: {0} by nick: {1} of message {2}]: {3}",
                    response.Channel,
                    response.Nick ?? "N/A",
                    response.Message,
                    ex);
            }
        }

        private void UnwireEvents()
        {
            _ircClient.LocalUser.JoinedChannel -= JoinedChannel;
            _ircClient.LocalUser.LeftChannel -= LeftChannel;
            _ircClient.LocalUser.MessageReceived -= IrcClient_PrivateMessageReceived;
            _ircClient.Registered -= ClientRegisteredWithIrcd;
            _ircClient.LocalUser.NoticeReceived -= NoticeReceived;
            _ircClient.RawMessageReceived -= IrcClient_RawMessageReceived;
            _ircClient.RawMessageSent -= IrcClient_RawMessageSent;
            _ircClient.Disconnected -= Disconnected;

            _ctcpClient.ActionReceived -= _ctcpClient_ActionReceived;
        }

        #region Event Handlers
        void JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            e.Channel.MessageReceived += IrcClient_PublicMessageReceived;
        }

        void LeftChannel(object sender, IrcChannelEventArgs e)
        {
            e.Channel.MessageReceived -= IrcClient_PublicMessageReceived;
        }

        void IrcClient_PublicMessageReceived(object sender, IrcMessageEventArgs e)
        {
            var sourceChannel = (IrcChannel)sender;
            var source = e.Source as IrcUser;

            if (source != null)
            {
                var sourceUser = source;
                var user = new User
                {
                    NickName = sourceUser.NickName,
                    HostName = sourceUser.HostName
                };

                OnPublicMessageReceived(user, sourceChannel.Name, e.Text);
            }
        }

        void IrcClient_PrivateMessageReceived(object sender, IrcMessageEventArgs e)
        {
            var sourceUser = (IrcUser)e.Source;
            var user = new User
            {
                NickName = sourceUser.NickName,
                HostName = sourceUser.HostName
            };

            OnPrivateMessageReceived(user, e.Text);
        }

        void _ctcpClient_ActionReceived(object sender, CtcpMessageEventArgs e)
        {
            var source = e.Source as IrcUser;

            if (source != null)
            {
                var sourceChannel = sender as IrcChannel;

                if (sourceChannel != null)
                {
                    var user = new User
                    {
                        NickName = source.NickName,
                        HostName = source.HostName
                    };

                    OnPublicActionReceived(user, sourceChannel.Name, e.Text);
                }
                else
                {
                    var user = new User
                    {
                        NickName = source.NickName,
                        HostName = source.HostName
                    };

                    OnPrivateActionReceived(user, e.Text);
                }
            }
        }

        void NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            var user = e.Source as IrcUser;

            if (_logger.IsTraceEnabled)
            {
                _logger.TraceFormat("{0} {1} sent a NOTICE: {2}", user.NickName, user.HostName, e.Text);
            }

            if (SupportsIdentification && NoticeIsExpectedServicesAgentMessage(user.NickName, e.Text))
            {
                JoinChannelsForNetwork();
            }
        }

        void ClientRegisteredWithIrcd(object sender, EventArgs e)
        {
            _ircClient.LocalUser.JoinedChannel += JoinedChannel;
            _ircClient.LocalUser.LeftChannel += LeftChannel;
            _ircClient.LocalUser.NoticeReceived += NoticeReceived;
            _ircClient.LocalUser.MessageReceived += IrcClient_PrivateMessageReceived;

            _ctcpClient.ActionReceived += _ctcpClient_ActionReceived;

            if (SupportsIdentification)
            {
                _ircClient.LocalUser.SendMessage("nickserv", String.Format("identify {0}", Network.AccountPassword));
            }
            else
            {
                JoinChannelsForNetwork();
            }
        }

        void IrcClient_RawMessageSent(object sender, IrcRawMessageEventArgs e)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.TraceFormat("Sent: {0}", e.RawContent);
            }
        }

        void IrcClient_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.TraceFormat("Received: {0}", e.RawContent);
            }

            if (e.Message.Command == "433")
            {
                var nickSuffix = _nickRetryCount.ToString();
                var newNick = _ircClient.LocalUser.NickName.Substring(0, _ircClient.LocalUser.NickName.Length - nickSuffix.Length) + nickSuffix;

                _ircClient.LocalUser.SetNickName(newNick);

                _nickRetryCount++;
            }
        }

        void Disconnected(object sender, EventArgs e)
        {
            var reconnectCount = 1;

            UnwireEvents();

            if (!_userInitiatedDisconnect)
            {
                while (!IsConnected)
                {
                    Connect();

                    if (!IsConnected)
                    {
                        _logger.WarnFormat("Failed whilst attempting reconnection attempt #{0}", reconnectCount);

                        Thread.Sleep(30000);
                        reconnectCount++;
                    }
                }
            }
        }
        #endregion
    }
}