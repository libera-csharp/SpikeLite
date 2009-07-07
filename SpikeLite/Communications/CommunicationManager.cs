/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using Sharkbite.Irc;
using SpikeLite.AccessControl;
using log4net.Ext.Trace;
using SpikeLite.Communications.IRC;
using System.Collections.Generic;

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
        /// A cache for storing user tokens that we'll use for determining the access level of incoming
        /// messages.
        /// </summary>
        private readonly UserTokenCache _userTokenCache = new UserTokenCache();

        /// <summary>
        /// Holds the connection object handed to us from SpikeLite, which handles reconnections etc.
        /// </summary>
        private Connection _connection;

        /// <summary>
        /// Stores our collaborator that we can use for generating access tokens on our messages we're going
        /// to pass around our system.
        /// </summary>
        public AuthenticationModule AuthHandler { get; set; }

        /// <summary>
        /// Stores our log4net logger.
        /// </summary>
        private readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(SpikeLite));

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
                    _connection.Listener.OnPublic -= Listener_OnPublic;
                    _connection.Listener.OnPrivate -= Listener_OnPrivate;
                }

                _connection = value;

                _connection.Listener.OnPublic += Listener_OnPublic;
                _connection.Listener.OnPrivate += Listener_OnPrivate;
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

        #region Message Passing

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
            else
            {
                _logger.WarnFormat("Received an unknown responsetype: {0}", response.ResponseType);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Attempts to parse an incoming message via a 1:N PRIVMSG.
        /// </summary>
        /// 
        /// <param name="userInfo">Information about the user who sent the message (nick, host etc).</param>
        /// <param name="channel">The channel the message came from.</param>
        /// <param name="message">The text of our message.</param>
        void Listener_OnPublic(UserInfo userInfo, string channel, string message)
        {
            UserToken userToken = _userTokenCache.RetrieveToken(userInfo);
            AuthToken authToken = AuthHandler.Authenticate(userToken);

            Request request = new Request
            {
                RequestFrom = authToken,
                Channel = channel,
                Nick = userInfo.Nick,
                RequestType = RequestType.Public,
                Addressee = userInfo.Nick,
                Message = message
            };

            OnRequestReceived(request);
        }

        /// <summary>
        /// Attempts to parse an incoming message via a 1:1 PRIVMSG.
        /// </summary>
        /// 
        /// <param name="userInfo">Information about the user who sent the message (nick, host etc).</param>
        /// <param name="message">The text of our message.</param>
        void Listener_OnPrivate(UserInfo userInfo, string message)
        {
            UserToken userToken = _userTokenCache.RetrieveToken(userInfo);
            AuthToken authToken = AuthHandler.Authenticate(userToken);
            
			Request request = new Request
            {
                RequestFrom = authToken,
                Channel = null,
                Nick = userInfo.Nick,
                RequestType = RequestType.Private,
                Message = message
            };

            OnRequestReceived(request);
        }

        /// <summary>
        /// Passes our internal message format to all subscribers.
        /// </summary>
        /// 
        /// <param name="request">Our message to pass.</param>
        protected virtual void OnRequestReceived(Request request)
        {
            if (_requestReceived != null)
            {
                _requestReceived(this, new RequestReceivedEventArgs(request));
            }
        }
        
        #endregion
    }
}