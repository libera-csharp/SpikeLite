/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using log4net.Ext.Trace;
using SpikeLite.Communications.Irc;
using SpikeLite.Communications.Irc.Configuration;
using SpikeLite.Communications.Messaging;
using SpikeLite.Shared.Communications;
using SpikeLite.Irc.ThresherIrc;

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
    public class CommunicationManager : ICommunicationManager
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

        private IIrcClient _ircClient = new IrcClient();

        /// <summary>
        /// Stores our log4net logger.
        /// </summary>
        private readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(CommunicationManager));

        #endregion

        #region Properties

        public BotStatus BotStatus
        {
            get
            {
                return this.BotContext.BotStatus;
            }
        }

        /// <summary>
        /// Gets our connection status.
        /// </summary>
        /// 
        /// <remarks>
        /// This will need to be refactored for multi-network support.
        /// </remarks>
        private bool IsConnected
        {
            get { return _ircClient != null && _ircClient.IsConnected; }
        }

        /// <summary>
        /// Gets or sets a collaborator we use to parse PRIVMSGes from our IRC library into internal <see cref="Request"/> structs.
        /// </summary>
        public IPrivmsgParser MessageParser
        {
            get
            {
                return _ircClient.MessageParser;
            }
            set
            {
                _ircClient.MessageParser = value;
            }
        }

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
        public bool SupportsIdentification
        {
            get
            {
                return _ircClient.SupportsIdentification;
            }
            set
            {
                _ircClient.SupportsIdentification = value;
            }
        }

        /// <summary>
        /// Gets or sets a reference to the bot's main context. Useful for doing things like shutting down.
        /// </summary>
        public SpikeLite BotContext { get; set; }

        #endregion

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
            _ircClient.Connect(this, NetworkList[0]);
        }

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
            _ircClient.Quit(quitMessage);
        }

        #endregion

        #region Outgoing Messages

        /// <summary>
        /// Attempts to send an outgoing message to the world.
        /// </summary>
        /// 
        /// <param name="response">A response object to send.</param>
        public void SendResponse(Response response)
        {
            _ircClient.SendResponse(response);
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
            _ircClient.JoinChannel(channelName);
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
            _ircClient.PartChannel(channelName);
        }

        /// <summary>
        /// Attempts to do a CTCP ACTION in a given target channel.
        /// </summary>
        /// 
        /// <param name="channelName">The channel target to attempt to do an action within.</param>
        /// <param name="emoteText">The text to combine with the action.</param>
        public void DoAction(string channelName, string emoteText)
        {
            _ircClient.DoAction(channelName, emoteText);
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