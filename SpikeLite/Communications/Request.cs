/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.AccessControl;

namespace SpikeLite.Communications
{
    public enum RequestType
    {
        Public,
        Private
    }

    public struct Request
    {
        #region Fields
    
        private AuthToken _sender;
        private string _channel;
        private string _nick;
        private RequestType _requestType;
        private string _message;
        
        #endregion

        #region Properties
        
        public AuthToken RequestFrom
        {
            get { return _sender; }
            set { _sender = value; } 
        }

        public string Channel
        {
            get { return _channel; }
            set { _channel = value; } 
        }

        public string Nick
        {
            get { return _nick; }
            set { _nick = value; }
        }

        public RequestType RequestType
        {
            get { return _requestType; }
            set { _requestType = value; } 
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; } 
        }
        
        #endregion

        public Request(AuthToken requestFrom, string channel, string nick, RequestType requestType, string message)
        {
            _sender = requestFrom;
            _channel = channel;
            _nick = nick;
            _requestType = requestType;
            _message = message;
        }

        #region CreateResponse
        
        public Response CreateResponse( ResponseType maxResponse )
        {
            return CreateResponse(maxResponse, "");
        }

        public Response CreateResponse( ResponseType maxResponse, string message )
        {
            ResponseType responseType = GetResponseType(maxResponse);

            return new Response(_channel, _nick, responseType, message);
        }

        public Response CreateResponse( ResponseType maxResponse, string message, object arg1 )
        {
            return CreateResponse(maxResponse, string.Format(message, arg1));
        }

        public Response CreateResponse( ResponseType maxResponse, string message, object arg1, object arg2 )
        {
            return CreateResponse(maxResponse, string.Format(message, arg1, arg2));
        }

        public Response CreateResponse( ResponseType maxResponse, string message, object arg1, object arg2, object arg3 )
        {
            return CreateResponse(maxResponse, string.Format(message, arg1, arg2, arg3));
        }

        public Response CreateResponse( ResponseType maxResponse, string message, params object[] args )
        {
            return CreateResponse(maxResponse, string.Format(message, args));
        }
        
        #endregion

        private ResponseType GetResponseType( ResponseType maxResponse )
        {
            if (maxResponse == ResponseType.Private || _requestType == RequestType.Private)
            {
                return ResponseType.Private;
            }
            else
            {
                return ResponseType.Public;
            }
        }
    }
}