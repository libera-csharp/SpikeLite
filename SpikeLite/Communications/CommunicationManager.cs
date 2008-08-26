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

namespace SpikeLite.Communications
{
    public class RequestReceivedEventArgs : EventArgs
    {
        private Request _request;

        public Request Request
        {
            get { return _request; }
        }

        public RequestReceivedEventArgs(Request request)
        {
            _request = request;
        }
    }

    public class CommunicationManager
    {
        #region Data Members

        private event EventHandler<RequestReceivedEventArgs> _requestReceived;
        private UserTokenCache _userTokenCache;

        private Connection _connection;
        private AuthenticationModule _authenticationModule;

        #endregion

        public event EventHandler<RequestReceivedEventArgs> RequestReceived
        {
            add { _requestReceived += value; }
            remove { _requestReceived -= value; }
        }

        public CommunicationManager(Connection connection, AuthenticationModule authenticationModule)
        {
            _connection = connection;
            _authenticationModule = authenticationModule;
            _userTokenCache = new UserTokenCache(connection);

            _connection.Listener.OnPublic += new PublicMessageEventHandler(Listener_OnPublic);
            _connection.Listener.OnPrivate += new PrivateMessageEventHandler(Listener_OnPrivate);
        }
        
        public void SendResponse(Response response)
        {
            ResponseType responseType = response.ResponseType;

            if ( response.Channel == null )
            {
                //cannot send a public message in response to a private message
                //TODO: log that this error occured and was corrected
                responseType = ResponseType.Private;
            }

            if (response.ResponseType == ResponseType.Public)
            {
                _connection.Sender.PublicMessage(response.Channel, response.Message);
            }
            else if (response.ResponseType == ResponseType.Private)
            {
                //TODO: check that response.RespondTo is in the same channel as data.NickName
                //TODO: check that the bot can see the response.RespondTo
                _connection.Sender.PrivateMessage(response.Nick, response.Message);
            }
            else
            {
                throw new Exception("Unknown ResponseType.");
            }
        }

        #region Event Handlers

        void Listener_OnPublic(UserInfo userInfo, string channel, string message)
        {
            UserToken userToken = _userTokenCache.RetrieveToken(userInfo);
            AuthToken authToken = _authenticationModule.Authenticate(userToken);
            Request request = new Request(authToken, channel, userInfo.Nick, RequestType.Public, message);

            OnRequestReceived(request);
        }

        void Listener_OnPrivate(UserInfo userInfo, string message)
        {
            UserToken userToken = _userTokenCache.RetrieveToken(userInfo);
            AuthToken authToken = _authenticationModule.Authenticate(userToken);
            Request request = new Request(authToken, null, userInfo.Nick, RequestType.Private, message);

            OnRequestReceived(request);
        }

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