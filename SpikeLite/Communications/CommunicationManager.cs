/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
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
        public Request Request { get; private set; }

        public RequestReceivedEventArgs(Request request)
        {
            Request = request;
        }
    }

    /// <summary>
    /// Our communications manager handles sending of messages between the external world and our internal
    /// systems.
    /// </summary>
    public class CommunicationManager
    {
        /// <summary>
        /// Wraps our incoming messages with a Request object.
        /// </summary>
        private event EventHandler<RequestReceivedEventArgs> _requestReceived;

        /// <summary>
        /// Holds the connection object handed to us from SpikeLite, which handles reconnections etc.
        /// </summary>
        private Connection _connection;

        /// <summary>
        /// Stores our log4net logger.
        /// </summary>
        private readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(SpikeLite));

        /// <summary>
        /// Gets or sets a collaborator we use to parse PRIVMSGes from our IRC library into internal <see cref="Request"/> structs.
        /// </summary>
        public IPrivmsgParser MessageParser { get; set; }

        /// <summary>
        /// Allows the getting or setting of our connection object.
        /// </summary>
        /// 
        /// <remarks>
        /// Attempting to set this will automatically attempt to unsubscribe all events IFF we've got something
        /// assigned to this property. We will then attempt to subscribe our event handlers to the new reference.
        /// </remarks>
        public Connection Connection
        {
            get { return _connection; }

            set
            {
                if (null != _connection)
                {
                    _connection.Listener.OnPublic -= MessageParser.HandleMultiTargetMessage;
                    _connection.Listener.OnPrivate -= MessageParser.HandleSingleTargetMessage;
                }

                _connection = value;

                _connection.Listener.OnPublic += MessageParser.HandleMultiTargetMessage;
                _connection.Listener.OnPrivate += MessageParser.HandleSingleTargetMessage;
            }
        }

        /// <summary>
        /// Gets or sets the list of networks to consider connecting to.
        /// </summary>
        public IList<Network> NetworkList
        {
            get; set;
        }


        public event EventHandler<RequestReceivedEventArgs> RequestReceived
        {
            add { _requestReceived += value; }
            remove { _requestReceived -= value; }
        }

        #region Outgoing Messages

        /// <summary>
        /// Attempts to send an outgoing message to the world.
        /// </summary>
        /// 
        /// <param name="response">A response object to send.</param>
        public void SendResponse(Response response)
        {
            if (response.ResponseType == ResponseType.Public)
            {
                _connection.Sender.PublicMessage(response.Channel, response.Message);
            }

            else if (response.ResponseType == ResponseType.Private)
            {
                if (response.Channel == null)
                {
                    _logger.WarnFormat("Attempted to send a public-targeted message in response to a private message. Nick: {0} Message: {1}", 
                                       response.Nick, 
                                       response.Message);
                }
                
                _connection.Sender.PrivateMessage(response.Nick, response.Message);
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
            if (_requestReceived != null)
            {
                _requestReceived(this, new RequestReceivedEventArgs(request));
            }
        }
        
        #endregion
    }
}