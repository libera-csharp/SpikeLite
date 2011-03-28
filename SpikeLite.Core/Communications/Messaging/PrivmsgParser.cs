/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using Sharkbite.Irc;
using SpikeLite.AccessControl;

namespace SpikeLite.Communications.Messaging
{
    /// <summary>
    /// A concrete implementation of the <see cref="IPrivmsgParser"/> contract, this class also handles authentication of users.
    /// </summary>
    class PrivmsgParser : IPrivmsgParser
    {
        /// <summary>
        /// Gets or sets a <see cref="CommunicationManager"/> that we can use for distributing our parsed messages to other components
        /// within the system.
        /// </summary>
        public CommunicationManager CommunicationManager { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="IAuthenticationModule"/> implementaiton that handles shoving authentication tokens into our parsed messages.
        /// </summary>
        public IAuthenticationModule AuthHandler { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="UserTokenCache"/> repository of cached user tokens for our <see cref="AuthHandler"/>.
        /// </summary>
        private UserTokenCache UserTokenCache { get; set; }

        public void HandleMultiTargetMessage(UserInfo user, string channel, string message)
        {
            IUserToken userToken = UserTokenCache.RetrieveToken(user);
            AuthToken authToken = AuthHandler.Authenticate(userToken);

            Request request = new Request
            {
                RequestFrom = authToken,
                Channel = channel,
                Nick = user.Nick,
                RequestType = RequestType.Public,
                Addressee = user.Nick,
                Message = message
            };

            CommunicationManager.HandleRequestReceived(request);
        }

        public void HandleSingleTargetMessage(UserInfo user, string message)
        {
            IUserToken userToken = UserTokenCache.RetrieveToken(user);
            AuthToken authToken = AuthHandler.Authenticate(userToken);

            Request request = new Request
            {
                RequestFrom = authToken,
                Channel = null,
                Nick = user.Nick,
                RequestType = RequestType.Private,
                Message = message
            };

            CommunicationManager.HandleRequestReceived(request);
        }
    }
}
