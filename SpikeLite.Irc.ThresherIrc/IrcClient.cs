using System;
using System.Threading;
using log4net.Ext.Trace;
using Sharkbite.Irc;
using SpikeLite.Communications;
using SpikeLite.Communications.Irc;
using SpikeLite.Communications.Irc.Configuration;
using SpikeLite.Shared.Communications;

namespace SpikeLite.Irc.ThresherIrc
{
    public class IrcClient : IrcClientBase, IIrcClient
    {
        private Connection _ircConnection;
        private readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(IrcClient));

        public override bool IsConnected
        {
            get 
            { 
                return _ircConnection.Connected; 
            }
        }

        public override void Connect(ICommunicationManager communicationManager, Network network)
        {
            this.CommunicationManager = communicationManager;
            this.Network = network;

            this.Connect();
        }

        public override void DoAction(string channelName, string emoteText)
        {
            _ircConnection.Sender.Action(channelName, emoteText);
        }

        public override void JoinChannel(string channelName)
        {
            _ircConnection.Sender.Join(channelName);
        }

        public override void PartChannel(string channelName)
        {
            _ircConnection.Sender.Part(channelName);
        }

        public override void Quit()
        {
            _ircConnection.Disconnect(string.Empty);
        }

        public override void Quit(string message)
        {
            _ircConnection.Disconnect(message);
        }

        public override void SendResponse(Response response)
        {
            try
            {
                if (response.ResponseType == ResponseType.Private)
                {
                    _ircConnection.Sender.PrivateMessage(response.Nick, response.Message);
                }
                else if (response.ResponseType == ResponseType.Public)
                {
                    _ircConnection.Sender.PublicMessage(response.Channel, response.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.WarnFormat("Caught an exception trying to SendResponse on [channel: {0} by nick: {1} of message {2}]: {3}",
                                   response.Channel, response.Nick ?? "N/A", response.Message, ex);
            }
        }

        private void Connect()
        {
            ConnectionArgs connectionArgs = new ConnectionArgs()
            {
                Nick = this.Network.BotNickname,
                RealName = this.Network.BotRealname,
                UserName = this.Network.BotUsername,
                Hostname = this.Network.ServerList[0].Host,
                Port = this.Network.ServerList[0].Port ?? 6667
            };

            _ircConnection = new Connection(connectionArgs, true, false);
            _ircConnection.OnRawMessageReceived += _ircConnection_OnRawMessageReceived;
            _ircConnection.OnRawMessageSent += _ircConnection_OnRawMessageSent;
            _ircConnection.Listener.OnRegistered += Listener_OnRegistered;
            _ircConnection.Listener.OnPrivateNotice += Listener_OnPrivateNotice;
            _ircConnection.Listener.OnPublic += Listener_OnPublic;
            _ircConnection.Listener.OnPrivate += Listener_OnPrivate;
            _ircConnection.Listener.OnDisconnected += Listener_OnDisconnected;

            _ircConnection.Connect();
        }

        private void UnwireEvents()
        {
            _ircConnection.OnRawMessageReceived -= _ircConnection_OnRawMessageReceived;
            _ircConnection.OnRawMessageSent -= _ircConnection_OnRawMessageSent;
            _ircConnection.Listener.OnRegistered -= Listener_OnRegistered;
            _ircConnection.Listener.OnPrivateNotice -= Listener_OnPrivateNotice;
            _ircConnection.Listener.OnPublic -= Listener_OnPublic;
            _ircConnection.Listener.OnPrivate -= Listener_OnPrivate;
            _ircConnection.Listener.OnDisconnected -= Listener_OnDisconnected;
        }

        void _ircConnection_OnRawMessageReceived(string message)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.TraceFormat("Received: {0}", message);
            }
        }

        void _ircConnection_OnRawMessageSent(string message)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.TraceFormat("Sent: {0}", message);
            }
        }

        void Listener_OnRegistered()
        {
            if (SupportsIdentification)
            {
                _ircConnection.Sender.PrivateMessage("nickserv", string.Format("identify {0}", Network.AccountPassword));
            }
            else
            {
                JoinChannelsForNetwork();
            }
        }

        void Listener_OnPrivateNotice(UserInfo user, string notice)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.TraceFormat("{0} {1} sent a NOTICE: {2}", user.Nick, user.Hostname, notice);
            }

            if (SupportsIdentification && NoticeIsExpectedServicesAgentMessage(user.Nick, notice))
            {
                JoinChannelsForNetwork();
            }
        }

        void Listener_OnPublic(UserInfo userInfo, string channel, string message)
        {
            var user = new User
            {
                NickName = userInfo.Nick,
                HostName = userInfo.Hostname
            };

            MessageParser.HandleMultiTargetMessage(user, channel, message);
        }

        void Listener_OnPrivate(UserInfo userInfo, string message)
        {
            var user = new User
            {
                NickName = userInfo.Nick,
                HostName = userInfo.Hostname
            };

            MessageParser.HandleSingleTargetMessage(user, message);
        }

        void Listener_OnDisconnected()
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
    }
}