using System;
using System.Linq;
using System.Threading;

using Cadenza.Collections;

using IrcDotNet;
using IrcDotNet.Ctcp;

using log4net.Ext.Trace;
using SpikeLite.Communications.Irc;
using SpikeLite.Shared.Communications;
using SpikeLite.Communications.Irc.Configuration;
using SpikeLite.Communications;
using SpikeLite.Communications.Messaging;

namespace SpikeLite.Irc.IrcDotNet
{
    public class IrcClient : IrcClientBase, IIrcClient
    {
        #region Data members
        private global::IrcDotNet.IrcClient _ircClient;
        private CtcpClient _ctcpClient;
        private readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(IrcClient));
        private int nickRetryCount = 0;
        #endregion

        public override bool IsConnected
        {
            get { return _ircClient != null && _ircClient.IsConnected; }
        }

        public override void Connect(ICommunicationManager communicationManager, Network network)
        {
            this.CommunicationManager = communicationManager;
            this.Network = network;
            this.Server = this.Network.ServerList.First();

            this.Connect();
        }

        private void Connect()
        {
            _ircClient = new global::IrcDotNet.IrcClient();
            _ctcpClient = new CtcpClient(_ircClient);

            _ircClient.Registered += ClientRegisteredWithIrcd;
            _ircClient.RawMessageReceived += IrcClient_RawMessageReceived;
            _ircClient.RawMessageSent += IrcClient_RawMessageSent;

            _ircClient.Connect(this.Server.Host, this.Server.Port ?? 6667, false, new IrcUserRegistrationInfo
            {
                NickName = this.Network.BotNickname,
                UserName = this.Network.BotUsername,
                RealName = this.Network.BotRealname
            });

            _ircClient.Disconnected += Disconnected;
        }

        public override void DoAction(string channelName, string emoteText)
        {
            _ctcpClient.SendAction(_ircClient
                .Channels
                .Where(c => c.Name.Equals(channelName, StringComparison.OrdinalIgnoreCase))
                .Single(),
                emoteText);
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
                _logger.TraceFormat("Quitting with no message.");
                _ircClient.Quit();
            }
        }

        public override void Quit(string message)
        {
            if (IsConnected)
            {
                _logger.TraceFormat("Sending quit message {0}.", message);
                _ircClient.Quit(message);
            }
        }

        public override void SendResponse(Response response)
        {
            try
            {
                if (response.ResponseType == ResponseType.Public)
                {
                    _ircClient.LocalUser.SendMessage(response.Channel, response.Message);
                }
                else if (response.ResponseType == ResponseType.Private)
                {
                    _ircClient.LocalUser.SendMessage(response.Nick, response.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.WarnFormat("Caught an exception trying to SendResponse on [channel: {0} by nick: {1} of message {2}]: {3}",
                                   response.Channel, response.Nick ?? "N/A", response.Message, ex);
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

            if (e.Source is IrcUser)
            {
                var sourceUser = (IrcUser)e.Source;

                var user = new User
                {
                    NickName = sourceUser.NickName,
                    HostName = sourceUser.HostName
                };

                MessageParser.HandleMultiTargetMessage(user, sourceChannel.Name, e.Text);
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

            MessageParser.HandleSingleTargetMessage(user, e.Text);
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
                string nickSuffix = nickRetryCount.ToString();
                string newNick = _ircClient.LocalUser.NickName
                    .Substring(0, _ircClient.LocalUser.NickName.Length - nickSuffix.Length) + nickSuffix;

                _ircClient.LocalUser.SetNickName(newNick);

                nickRetryCount++;
            }
        }

        void Disconnected(object sender, EventArgs e)
        {
            int reconnectCount = 1;

            UnwireEvents();

            if (CommunicationManager.BotStatus == BotStatus.Started)
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