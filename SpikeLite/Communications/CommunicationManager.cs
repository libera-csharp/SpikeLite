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
        public Request Request { get; private set; }

        public RequestReceivedEventArgs(Request request)
        {
            Request = request;
        }
    }

    public class CommunicationManager
    {
        #region Data Members

        private event EventHandler<RequestReceivedEventArgs> _requestReceived;
        private readonly UserTokenCache _userTokenCache;

        private readonly Connection _connection;
        private readonly AuthenticationModule _authenticationModule;

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

            _connection.Listener.OnPublic += Listener_OnPublic;
            _connection.Listener.OnPrivate += Listener_OnPrivate;
        }
        
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
                    //cannot send a public message in response to a private message
                    //TODO: log that this error occured
                }
                
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

            Request request = new Request()
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

        void Listener_OnPrivate(UserInfo userInfo, string message)
        {
            UserToken userToken = _userTokenCache.RetrieveToken(userInfo);
            AuthToken authToken = _authenticationModule.Authenticate(userToken);
            
			Request request = new Request()
            {
                RequestFrom = authToken,
                Channel = null,
                Nick = userInfo.Nick,
                RequestType = RequestType.Private,
                Message = message
            };

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