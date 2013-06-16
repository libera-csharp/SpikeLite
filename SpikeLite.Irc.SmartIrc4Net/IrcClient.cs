using System;
using System.Linq;
using System.Threading;
using log4net.Ext.Trace;
using SpikeLite.Communications;
using SpikeLite.Communications.Irc;
using SIRC4N = Meebey.SmartIrc4net;

namespace SpikeLite.Irc.SmartIrc4Net
{
    public class IrcClient : IrcClientBase
    {
        private SIRC4N.IrcClient _ircClient = new SIRC4N.IrcClient();
        private Thread listenThread;
        private readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(IrcClient));
        private bool _userInitiatedDisconnect;

        public override bool IsConnected { get { return _ircClient.IsConnected; } }

        public override void Connect(Communications.Irc.Configuration.Network network)
        {
            Network = network;
            Server = Network.ServerList.First();

            Connect();
        }

        private void Connect()
        {
            _ircClient = new SIRC4N.IrcClient();

            _ircClient.OnDisconnected += _ircClient_OnDisconnected;
            _ircClient.OnRawMessage += _ircClient_OnRawMessage;
            _ircClient.OnRegistered += _ircClient_OnRegistered;
            _ircClient.OnChannelAction += _ircClient_OnChannelAction;
            _ircClient.OnChannelMessage += _ircClient_OnChannelMessage;
            _ircClient.OnQueryAction += _ircClient_OnQueryAction;
            _ircClient.OnQueryMessage += _ircClient_OnQueryMessage;
            _ircClient.OnQueryNotice += _ircClient_OnQueryNotice;

            _ircClient.Connect(Server.Host, Server.Port ?? 6667);
            _ircClient.Login(Network.BotNickname, Network.BotRealname);

            listenThread = new Thread(_ircClient.Listen);
            listenThread.Start();
        }

        private void UnwireEvents()
        {
            _ircClient.OnDisconnected -= _ircClient_OnDisconnected;
            _ircClient.OnRawMessage -= _ircClient_OnRawMessage;
            _ircClient.OnRegistered -= _ircClient_OnRegistered;
            _ircClient.OnChannelAction -= _ircClient_OnChannelAction;
            _ircClient.OnChannelMessage -= _ircClient_OnChannelMessage;
            _ircClient.OnQueryAction -= _ircClient_OnQueryAction;
            _ircClient.OnQueryMessage -= _ircClient_OnQueryMessage;
            _ircClient.OnQueryNotice -= _ircClient_OnQueryNotice;
        }

        public override void DoAction(string channelName, string emoteText)
        {
            _ircClient.SendMessage(SIRC4N.SendType.Action, channelName, emoteText);
        }

        public override void JoinChannel(string channelName)
        {
            _ircClient.RfcJoin(channelName);
        }

        public override void PartChannel(string channelName)
        {
            _ircClient.RfcPart(channelName);
        }

        public override void Quit()
        {
            if (IsConnected)
            {
                _userInitiatedDisconnect = true;
                _ircClient.RfcQuit();
            }
        }

        public override void Quit(string message)
        {
            if (IsConnected)
            {
                _userInitiatedDisconnect = true;
                _ircClient.RfcQuit(message);
            }
        }

        public override void SendResponse(Response response)
        {
            try
            {
                if (response.ResponseType == ResponseType.Action)
                {
                    if (response.ResponseTargetType == ResponseTargetType.Private)
                    {
                        _ircClient.SendMessage(SIRC4N.SendType.Message, response.Nick, response.Message);
                    }
                    else if (response.ResponseTargetType == ResponseTargetType.Public)
                    {
                        _ircClient.SendMessage(SIRC4N.SendType.Action, response.Channel, response.Message);
                    }
                }
                else if (response.ResponseType == ResponseType.Message)
                {
                    if (response.ResponseTargetType == ResponseTargetType.Private)
                    {
                        _ircClient.SendMessage(SIRC4N.SendType.Message, response.Nick, response.Message);
                    }
                    else if (response.ResponseTargetType == ResponseTargetType.Public)
                    {
                        _ircClient.SendMessage(SIRC4N.SendType.Message, response.Channel, response.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.WarnFormat("Caught an exception trying to SendResponse on [channel: {0} by nick: {1} of message {2}]: {3}",
                                   response.Channel, response.Nick ?? "N/A", response.Message, ex);
            }
        }

        void _ircClient_OnDisconnected(object sender, EventArgs e)
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

        void _ircClient_OnRawMessage(object sender, SIRC4N.IrcEventArgs e)
        {
            if (_ircClient.IsMe(e.Data.From))
            {
                if (_logger.IsTraceEnabled)
                {
                    _logger.TraceFormat("Sent: {0}", e.Data.RawMessage);
                }
            }
            else
            {
                if (_logger.IsTraceEnabled)
                {
                    _logger.TraceFormat("Received: {0}", e.Data.RawMessage);
                }
            }
        }

        void _ircClient_OnRegistered(object sender, EventArgs e)
        {
            if (SupportsIdentification)
            {
                _ircClient.SendMessage(SIRC4N.SendType.Message, "nickserv", String.Format("identify {0}", Network.AccountPassword));
            }
            else
            {
                JoinChannelsForNetwork();
            }
        }

        void _ircClient_OnChannelAction(object sender, SIRC4N.ActionEventArgs e)
        {
            var user = new User
            {
                NickName = e.Data.Nick,
                HostName = e.Data.Host
            };

            OnPublicActionReceived(user, e.Data.Channel, e.Data.Message);
        }

        void _ircClient_OnChannelMessage(object sender, SIRC4N.IrcEventArgs e)
        {
            var user = new User
            {
                NickName = e.Data.Nick,
                HostName = e.Data.Host
            };

            OnPublicMessageReceived(user, e.Data.Channel, e.Data.Message);
        }

        void _ircClient_OnQueryAction(object sender, SIRC4N.ActionEventArgs e)
        {
            var user = new User
            {
                NickName = e.Data.Nick,
                HostName = e.Data.Host
            };

            OnPrivateActionReceived(user, e.Data.Message);
        }

        void _ircClient_OnQueryMessage(object sender, SIRC4N.IrcEventArgs e)
        {
            var user = new User
            {
                NickName = e.Data.Nick,
                HostName = e.Data.Host
            };

            OnPrivateMessageReceived(user, e.Data.Message);
        }

        void _ircClient_OnQueryNotice(object sender, SIRC4N.IrcEventArgs e)
        {
            if (SupportsIdentification && NoticeIsExpectedServicesAgentMessage(e.Data.Nick, e.Data.Message))
            {
                JoinChannelsForNetwork();
            }
        }
    }
}