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
        public AuthToken RequestFrom { get; set; }
        public string Channel { get; set; }
        public string Nick { get; set; }
        public RequestType RequestType { get; set; }
        public string Addressee { get; set; }
        public string Message { get; set; }

        #region CreateResponse
        
        public Response CreateResponse( ResponseType maxResponse )
        {
            return CreateResponse(maxResponse, "");
        }

        public Response CreateResponse( ResponseType maxResponse, string message )
        {
            ResponseType responseType = GetResponseType(maxResponse);

            return new Response()
            {
                Channel = Channel,
                Nick = Nick,
                ResponseType = responseType,
                Message = message
            };
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
            if (maxResponse == ResponseType.Private || RequestType == RequestType.Private)
            {
                return ResponseType.Private;
            }

            return ResponseType.Public;
        }
    }
}