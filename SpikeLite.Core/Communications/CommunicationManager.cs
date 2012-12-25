/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Ext.Trace;
using SpikeLite.Communications.Irc;
using SpikeLite.Communications.Irc.Configuration;
using SpikeLite.Communications.Messaging;

namespace SpikeLite.Communications
{
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

        private IIrcClient _ircClient;
        private bool _isStarted;

        /// <summary>
        /// Stores our log4net logger.
        /// </summary>
        private readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(CommunicationManager));

        #endregion

        #region Properties

        //private Type _ircClientType = typeof(IrcClient);
        //public Type IrcClientType { get { return _ircClientType; } set { _ircClientType = value; } }
        public Type IrcClientType { get; set; }

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
        /// </remarks>
        public void Connect()
        {
            if (!_isStarted)
            {
                _ircClient = (IIrcClient)Activator.CreateInstance(IrcClientType);
                _ircClient.SupportsIdentification = SupportsIdentification;
                _ircClient.PublicMessageReceived += _ircClient_PublicMessageReceived;
                _ircClient.PrivateMessageReceived += _ircClient_PrivateMessageReceived;
                _ircClient.Connect(NetworkList.First());

                _isStarted = true;
            }
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
            if (_isStarted)
            {
                _ircClient.Quit(quitMessage);
                _ircClient.PublicMessageReceived -= _ircClient_PublicMessageReceived;
                _ircClient.PrivateMessageReceived -= _ircClient_PrivateMessageReceived;

                _isStarted = false;
            }
        }

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

        void _ircClient_PublicMessageReceived(object sender, PublicMessageReceivedEventArgs e)
        {
            MessageParser.HandleMultiTargetMessage(e.User, e.ChannelName, e.Message);
        }

        void _ircClient_PrivateMessageReceived(object sender, PrivateMessageReceivedEventArgs e)
        {
            MessageParser.HandleSingleTargetMessage(e.User, e.Message);
        }

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