/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.AccessControl;

namespace SpikeLite.Communications
{
    /// <summary>
    /// This enum represents the source types of request possible, in this case either a 1:1 or 1:N message.
    /// </summary>
    public enum RequestSourceType
    {
        /// <summary>
        /// This is a 1:N message, coming from a channel.
        /// </summary>
        Public,

        /// <summary>
        /// This is a 1:1 message, coming from a single user.
        /// </summary>
        Private
    }

    /// <summary>
    /// This enum represents the types of request possible, in this case either an action or a message.
    /// </summary>
    public enum RequestType
    {
        /// <summary>
        /// This is an action, coming from a source.
        /// </summary>
        Action,

        /// <summary>
        /// This is a message, coming from a source.
        /// </summary>
        Message
    }

    /// <summary>
    /// This struct contains information about a message being sent to the bot.
    /// </summary>
    public struct Request
    {
        #region Properties

        /// <summary>
        /// Gets or sets an authentication token containing authorization and other details.
        /// </summary>
        public AuthToken RequestFrom { get; set; }

        /// <summary>
        /// Gets or sets the channel from which the request was sent.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets the nick of the user from which the request was sent.
        /// </summary>
        public string Nick { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RequestSourceType"/> of the message.
        /// </summary>
        public RequestSourceType RequestSourceType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RequestType"/> of the message.
        /// </summary>
        public RequestType RequestType { get; set; }

        /// <summary>
        /// Gets or sets the name of the user to respond to. This may, or may not, be the nickname of the request originator.
        /// </summary>
        public string Addressee { get; set; }

        /// <summary>
        /// Gets or sets the actual message payload of the given request.
        /// </summary>
        public string Message { get; set; }

        #endregion

        #region CreateResponse

        /// <summary>
        /// Creates a response for a given response type.
        /// </summary>
        /// 
        /// <param name="maxResponse">The type of response to yield.</param>
        /// 
        /// <returns>A response structure capable of being sent along the application as a message.</returns>
        public Response CreateResponse(ResponseType maxResponse)
        {
            return CreateResponse(maxResponse, "");
        }


        /// <summary>
        /// Creates a response for a given response type with a given message payload.
        /// </summary>
        /// 
        /// <param name="maxResponse">The type of response to yield.</param>
        /// <param name="message">The message payload to deliver.</param>
        /// 
        /// <returns>A response structure capable of being sent along the application as a message.</returns>
        public Response CreateResponse(ResponseType maxResponse, string message)
        {
            var responseType = GetResponseType(maxResponse);

            return new Response
            {
                Channel = Channel,
                Nick = Nick,
                ResponseType = responseType,
                Message = message
            };
        }

        public Response CreateResponse(ResponseType maxResponse, string message, object arg1)
        {
            return CreateResponse(maxResponse, string.Format(message, arg1));
        }

        public Response CreateResponse(ResponseType maxResponse, string message, object arg1, object arg2)
        {
            return CreateResponse(maxResponse, string.Format(message, arg1, arg2));
        }

        public Response CreateResponse(ResponseType maxResponse, string message, object arg1, object arg2, object arg3)
        {
            return CreateResponse(maxResponse, string.Format(message, arg1, arg2, arg3));
        }

        public Response CreateResponse(ResponseType maxResponse, string message, params object[] args)
        {
            return CreateResponse(maxResponse, string.Format(message, args));
        }

        #endregion

        private ResponseType GetResponseType(ResponseType maxResponse)
        {
            if (maxResponse == ResponseType.Private || RequestSourceType == RequestSourceType.Private)
            {
                return ResponseType.Private;
            }

            return ResponseType.Public;
        }
    }
}