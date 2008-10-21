/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

namespace SpikeLite.Communications
{
    public enum ResponseType
    { 
        Public,
        Private
    }

    public struct Response
    {
        #region Fields
        
        private string _channel;
        private string _nick;
        private ResponseType _responseType;
        private string _message;
        
        #endregion

        #region Properties
        
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

        public ResponseType ResponseType
        {
            get { return _responseType; }
            set { _responseType = value; }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        
        #endregion

        public Response(string channel, string nick, ResponseType responseType, string message)
        {
            _channel = channel;
            _nick = nick;
            _responseType = responseType;
            _message = message;
        }
    }
}