/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Threading;
using Cadenza.Collections;
using Sharkbite.Irc;
using log4net.Ext.Trace;
using SpikeLite.Communications.IRC;
using System.Collections.Generic;
using SpikeLite.Communications.Messaging;

namespace SpikeLite.Communications
{
    /// <summary>
    /// Defines a set of event arguments for a message being received.
    /// </summary>
    public class RequestReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Holds the <see cref="Request"/> for the inbound message.
        /// </summary>
        public Request Request { get; private set; }

        /// <summary>
        /// Constructs an instance of our request arguments, which basically wrap a <see cref="Request"/>.
        /// </summary>
        /// 
        /// <param name="request">The <see cref="Request"/> representing our inbound message.</param>
        public RequestReceivedEventArgs(Request request)
        {
            Request = request;
        }
    }

    // TODO: Kog 11/23/2009 - I removed a lot of behavior from SpikeLite that was network specific and placed it here. This isn't the proper place for some of the responsibilities,
    // TODO: Kog 11/23/2009 - we'll probably have to decompose this into two or more classes: there's communications, there's network-aware routing, there's network-aware behavior
    // TODO: Kog 11/23/2009 - and more. But for now this will suffice to decouple our engine.

    /// <summary>
    /// Our communications manager handles sending of messages between the external world and our internal
    /// systems.
    /// </summary>
    public class CommunicationManager
    {
        #region Events

        /// <summary>
        /// Backs <see cref="RequestReceived"/>
        /// </summary>
        private event EventHandler<RequestReceivedEventArgs> _requestReceived;

        /// <summary>
        /// Allows other subsystems within the bot to be handed messages as they come in.
        /// </summary>
        /// 
        /// <remarks>
        /// This event will hand subscribers fully parsed messages, formatted for consumption for modules within the bot. If you'd like the raw incoming 
        /// messages, you probably want to go look at the MessageParserAdvice bean in beans.xml, or the <see cref="PrivmsgParserAdvice"/> class for details.
        /// </remarks>
        public event EventHandler<RequestReceivedEventArgs> RequestReceived
        {
            add { _requestReceived += value; }
            remove { _requestReceived -= value; }
        }

        #endregion

        #region Data members 

        /// <summary>
        /// Holds the connection object handed to us from SpikeLite, which handles reconnections etc.
        /// </summary>
        private Connection _connection;

        /// <summary>
        /// Stores our log4net logger.
        /// </summary>
        private readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(CommunicationManager));

        #endregion 

        #region Properties

        /// <summary>
        /// Gets our connection status.
        /// </summary>
        /// 
        /// <remarks>
        /// This will need to be refactored for multi-network support.
        /// </remarks>
        public bool IsConnected
        {
            get { return _connection != null && _connection.Connected; }
        }

        /// <summary>
        /// Gets or sets a collaborator we use to parse PRIVMSGes from our IRC library into internal <see cref="Request"/> structs.
        /// </summary>
        public IPrivmsgParser MessageParser { get; set; }

        /// <summary>
        /// Gets or sets the list of networks to consider connecting to.
        /// </summary>
        public IList<Network> NetworkList { get; set; }

        /// <summary>
        /// Gets or sets whether the bot is set to identify itself with some sort of services agent or other infrastructure.
        /// </summary>
        /// 
        /// <remarks>
        /// TODO Kog: 11/15/2009 - This needs to be refactored for multi-network support, we should fold it into our network-aware context when we build said functionality.
        /// </remarks>
        public bool SupportsIdentification { get; set; }

        /// <summary>
        /// Gets or sets a reference to the bot's main context. Useful for doing things like shutting down.
        /// </summary>
        public SpikeLite BotContext { get; set; }

        #endregion

        #region Network/IRCD Related Behavior

        #region Things needing refactoring for multi-network support

        /// <summary>
        /// Attempt to connect to our pre-configured network. Does nothing if you're already connected.
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para>
        /// This will need to be refactored to allow multi-network support. This will perform a no-op
        /// if you are already connected.
        /// </para>
        ///
        /// <para>
        /// Unfortunately there's a rather annoying bug with Thresher's IRC library: when you re-use the same
        /// connection object, it maintains a cache of capability options, as sent as part of the registration
        /// preamble from the IRCD. There's no way (so far) to flush this cache, and it's not a very intelligent
        /// cache - it doesn't look for hits, only misses - so we wind up trying to cache something twice and
        /// an exception resulting from said behavior.
        /// </para>
        /// 
        /// <para>
        /// So... In order to support a reconnect ability you need to create a NEW connection object. This requires
        /// unsubscribing your old event handlers (to prevent memory leaks), and re-registering them. Painful duplication,
        /// crappy mixing of responsibility... name your reason why this is bad. Unfortunately there's not much choice.
        /// </para>
        /// 
        /// </remarks>
        public void Connect()
        {
            if (!IsConnected)
            {
                Network network = NetworkList[0];
                Server server = network.ServerList[0];

                ConnectionArgs connectionArgs = new ConnectionArgs
                {
                    Hostname = server.Host,
                    Nick = network.BotNickname,
                    Port = server.Port ?? 6667,
                    RealName = network.BotRealname,
                    UserName = network.BotUsername
                };

                _connection = new Connection(connectionArgs, true, true);

                // Make sure we can handle privmsg events.
                _connection.Listener.OnPublic += MessageParser.HandleMultiTargetMessage;
                _connection.Listener.OnPrivate += MessageParser.HandleSingleTargetMessage;

                // Subscribe our events
                _connection.Listener.OnRegistered += OnRegister;
                _connection.Listener.OnPrivateNotice += OnPrivateNotice;
                _connection.OnRawMessageReceived += OnRawMessageReceived;
                _connection.OnRawMessageSent += OnRawMessageSent;

                _connection.Connect();

                // Make sure we don't get any random disconnected events before we're connected
                _connection.Listener.OnDisconnected += OnDisconnect;
            }
        }

        // TODO: Kog 11/23/2009 - This sucks, re-write it. Make network-aware. Make it configurable as part of a network element.

        /// <summary>
        /// Handle disconnection. 
        /// </summary>
        /// 
        /// <remarks>
        /// We currently attempt to reconnect 5 times, given a 60 second stagger between connection attempts.
        /// </remarks>
        private void OnDisconnect()
        {
            int reconnectCount = 1;

            // Disconnect our event sources... see docs on the method, or on Connect.
            UnwireEvents();

            // Let's not reconnect when we're shutting down...
            if (BotContext.BotStatus == BotStatus.Started)
            {
                // Try and reconnect.
                while (!IsConnected && reconnectCount <= 5)
                {
                    Connect();

                    if (!IsConnected)
                    {
                        _logger.WarnFormat("Failed whilst attempting reconnection attempt #{0}", reconnectCount);

                        // TODO: Kog 11/23/2009 - Can we replace this with a timer of some sort?
                        Thread.Sleep(60000);
                        reconnectCount++;
                    }
                }

                // If we went through all the reconnect attempts, and we're still not getting anywhere kill the bot
                if (!IsConnected)
                {
                    BotContext.Shutdown("Cannot connect to any configured networks, shutting down.");
                }
            }
        }

        // TODO: Kog 11/23/2009 - Make network specific.

        /// <summary>
        /// Disconnect from our networks. 
        /// </summary>
        /// 
        /// <param name="quitMessage">Quit message to send to IRCD. Cannot be <b>null</b> or <b>empty</b>.</param>
        /// 
        /// <remarks>
        /// This will perform a no-op if you are not already connected.
        /// </remarks>
        public void Quit(string quitMessage)
        {
            if (IsConnected)
            {
                _logger.TraceFormat("Sending quit message {0}.", quitMessage);
                _connection.Disconnect(quitMessage);
            }
        }

        // TODO: Kog 11/23/2009 - Make network specific.

        /// <summary>
        /// Remove the wiring for all our event sources. Please see the documentation on <see cref="Connect"/> for details.
        /// </summary>
        private void UnwireEvents()
        {
            // Unsubscribe our communications manager.
            _connection.Listener.OnRegistered -= OnRegister;
            _connection.Listener.OnPrivateNotice -= OnPrivateNotice;
            _connection.OnRawMessageReceived -= OnRawMessageReceived;
            _connection.OnRawMessageSent -= OnRawMessageSent;
            _connection.Listener.OnDisconnected -= OnDisconnect;

            // Unsubscribe our AOP advice for catching PRIVMSGes.
            _connection.Listener.OnPublic -= MessageParser.HandleMultiTargetMessage;
            _connection.Listener.OnPrivate -= MessageParser.HandleSingleTargetMessage;
        }

        // TODO: Kog 11/23/2009 - Make network specific.

        /// <summary>
        /// Attempts to join all configured channels.
        /// </summary>
        private void JoinChannelsForNetwork()
        {
            NetworkList[0].ServerList[0].ChannelList.ForEach(channel =>
            {
                JoinChannel(channel.Name);
                _logger.InfoFormat("joining {0}...", channel.Name);
            });
        }

        // TODO: Kog 11/15/2009 - Refactor the following behavior to be network specific.

        /// <summary>
        /// The IRCD has notified us that "registration" is complete, and the MOTD has finished being sent.
        /// </summary>
        /// 
        /// <remarks>
        /// This is a good place to send your services registration. Some networks provide cloaks for users that are best
        /// set up upon connect. Consider this an analog to mIRC's onConnect. The bot currently blocks on a response, see
        /// <see cref="OnPrivateNotice"/>.
        /// </remarks>
        private void OnRegister()
        {
            if (SupportsIdentification)
            {
                _connection.Sender.PrivateMessage("nickserv", String.Format("identify {0}", NetworkList[0].AccountPassword));
            }
            else
            {
                // Don't bother blocking, we don't expect to identify.
                JoinChannelsForNetwork();
            }
        }

        // TODO: Kog 11/23/2009 - Refactor this into some sort of multi-network arrangement.

        /// <summary>
        /// We've recieved a NOTICE from the server.
        /// </summary>
        /// 
        /// <param name="user">A user representing the sender.</param>
        /// <param name="notice">The message being NOTICE'd.</param>
        /// 
        /// <remarks>
        /// We currently block upon connect between registration and receiving an authentication response. This is so that the
        /// real hostmask of the bot will not be revealed inadvertently. 
        /// </remarks>
        private void OnPrivateNotice(UserInfo user, string notice)
        {
            // Log the notice if we're set to the proper level.
            if (_logger.IsTraceEnabled)
            {
                _logger.TraceFormat("{0} {1} sent a NOTICE: {2}", user.Nick, user.Hostname, notice);
            }

            // If we support identification of some sort, we need to block until we receive confirmation of successful identification from services agents.
            // This keeps us from joining a channel before we have a vanity host.
            if (SupportsIdentification && NoticeIsExpectedServicesAgentMessage(user, notice))
            {
                JoinChannelsForNetwork();
            }
        }

        // TODO: Kog 11/23/2009 - Refactor this into some sort of multi-network arrangement.

        /// <summary>
        /// A convenience method for checking if we've received the notice we're blocking on from a services agent.
        /// </summary>
        /// 
        /// <param name="notice">The notice to digest.</param>
        /// <param name="user">The user sending the notice.</param>
        /// 
        /// <returns>True if we've been responded to by a services agent, else false.</returns>
        private bool NoticeIsExpectedServicesAgentMessage(UserInfo user, string notice)
        {
            // Make sure the message is coming from NickServ
            if (user.Nick.Equals("nickserv", StringComparison.OrdinalIgnoreCase))
            {
                return notice.StartsWith("You are now identified for", StringComparison.OrdinalIgnoreCase) ||
                       (notice.StartsWith("The nickname", StringComparison.OrdinalIgnoreCase) && notice.EndsWith("is not registered", StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

        #endregion

        #region Things Needing Refactoring Into AOP

        // TODO: Kog 11/23/2009 - Maybe make this a bit of "Before" advice like privmessages?

        /// <summary>
        /// We're sending a message. 
        /// </summary>
        /// 
        /// <param name="message">The text of the message sent to the IRCD.</param>
        /// 
        /// <remarks>
        /// The message text will be unformatted, as sent by the IRCD. May be RFC1459 compliant. 
        /// </remarks>
        private void OnRawMessageSent(string message)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.TraceFormat("Sent: {0}", message);
            }
        }

        // TODO: Kog 11/23/2009 - Maybe make this a bit of "Before" advice like privmessages?

        /// <summary>
        /// The IRCD has sent us a message that isn't a PRIVMSG or NOTICE.
        /// </summary>
        /// 
        /// <param name="message">The text of the message sent by the IRCD.</param>
        /// 
        /// <remarks>
        /// The message text will be unformatted, as sent by the IRCD. May be RFC1459 compliant. 
        /// </remarks>
        private void OnRawMessageReceived(string message)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.TraceFormat("Received: {0}", message);
            }
        }

        #endregion

        #endregion

        #region Outgoing Messages

        /// <summary>
        /// Attempts to send an outgoing message to the world.
        /// </summary>
        /// 
        /// <param name="response">A response object to send.</param>
        public void SendResponse(Response response)
        {
            try
            {
                if (response.ResponseType == ResponseType.Public)
                {
                    _connection.Sender.PublicMessage(response.Channel, response.Message);
                }

                else if (response.ResponseType == ResponseType.Private)
                {
                    _connection.Sender.PrivateMessage(response.Nick, response.Message);
                }                
            }
            catch (Exception ex)
            {
                _logger.WarnFormat("Caught an exception trying to SendResponse on [channel: {0} by nick: {1} of message {2}]: {3}", 
                                   response.Channel, response.Nick ?? "N/A", response.Message, ex);
            }

        }

        // TODO: Kog 11/15/2009 - We can probably refactor some of these methods to take advantage of optional params in .NET4.

        /// <summary>
        /// Attempts to join a channel by name.
        /// </summary>
        /// 
        /// <param name="channelName">The channel target to attempt to join.</param>
        /// 
        /// <remarks>
        /// This does not currently support sending a password.
        /// </remarks>
        public void JoinChannel(string channelName)
        {
            _connection.Sender.Join(channelName);
        }

        /// <summary>
        /// Attempts to part a channel by name.
        /// </summary>
        /// 
        /// <param name="channelName">The channel target to attempt to part.</param>
        /// 
        /// <remarks>
        /// This does not currently support multiple targets or a reason for parting. The IRCD issue whatever its default part message is.
        /// </remarks>
        public void PartChannel(string channelName)
        {
            _connection.Sender.Part(channelName);
        }

        /// <summary>
        /// Attempts to do a CTCP ACTION in a given target channel.
        /// </summary>
        /// 
        /// <param name="channelName">The channel target to attempt to do an action within.</param>
        /// <param name="emoteText">The text to combine with the action.</param>
        public void DoAction(string channelName, string emoteText)
        {
            _connection.Sender.Action(channelName, emoteText);
        }

        #endregion

        #region Incoming Messages

        /// <summary>
        /// Passes our internal message format to all subscribers.
        /// </summary>
        /// 
        /// <param name="request">Our message to pass.</param>
        public void HandleRequestReceived(Request request)
        {
            try
            {
                if (_requestReceived != null)
                {
                    _requestReceived(this, new RequestReceivedEventArgs(request));
                }              
            }
            catch (Exception ex)
            {
                _logger.WarnFormat("Caught an exception trying to HandleRequestReceived on [channel {0} nick {1} request type {2} message {3}]: {4}",
                                   request.Channel ?? "N/A", request.RequestFrom.User.DisplayName, request.RequestType, request.Message, ex);    
            }

        }
        
        #endregion
    }
}