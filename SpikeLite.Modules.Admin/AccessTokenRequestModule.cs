/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using SpikeLite.AccessControl;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Modules.Admin
{
    /// <summary>
    /// This class allows recognized users to generate a new access token.
    /// </summary>
    public class AccessTokenRequestModule : ModuleBase
    {
        /// <summary>
        /// Gets or sets a <see cref="IrcAuthenticationModule"/> we can use for operating with accounts.
        /// </summary>
        public IrcAuthenticationModule AuthenticationManager { get; set; }

        public override void HandleRequest(Request request)
        {
            if (request.RequestFrom.AccessLevel >= AccessLevel.Public && request.Message.StartsWith("~AccessToken", StringComparison.OrdinalIgnoreCase))
            {
                var splitMessage = request.Message.Split(' ');

                // We've got an operation.
                if (splitMessage.Length > 1)
                {
                    // TODO [Kog 07/23/2011] : We need to fix looking up who requested the token. This mechanism is pretty bad.

                    // Look up the user who sent the message.
                    var userToken = AuthenticationManager.ExchangeTokenForConcreteType<IrcUserToken>(request.RequestFrom.User);
                    var user = AuthenticationManager.FindHostByCloak(userToken.HostMask);

                    // Handle a user requesting a new access token.
                    if (splitMessage[1].Equals("request", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Try and pull the TTL for the token out of the args.
                        var timeToLive = 1;

                        if (splitMessage.Length > 2)
                        {
                            int.TryParse(splitMessage[2], out timeToLive);
                        }

                        var accessTokenExpiration = timeToLive == 0 ? DateTime.MaxValue : DateTime.Now.ToUniversalTime().AddDays(timeToLive);

                        // Generate a new GUID.
                        var accessToken = Guid.NewGuid();
                        var accessTokenCreationTime = DateTime.Now.ToUniversalTime();

                        // Slap it on the user's account.
                        user.AccessToken = accessToken.ToString();
                        user.AccessTokenIssueTime = accessTokenCreationTime;
                        user.AccessTokenExpiration = accessTokenExpiration;

                        // Update the account.
                        AuthenticationManager.UpdateHost(user);

                        // Hand it back.
                        Reply(request, ResponseTargetType.Private, "Your new access token is {0}.", accessToken);
                    }

                    // Handle a user asking for their current access token.
                    if (splitMessage[1].Equals("display", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseTargetType.Private,
                            user.AccessToken == null ?
                            "You have no access token. Please request one." :
                            String.Format("Your access token is {0} and was generated at {1} UTC. The token will expire at {2} UTC.", user.AccessToken, user.AccessTokenIssueTime, user.AccessTokenExpiration)));
                    }
                }
                else
                {
                    // The user used the command incorrectly.
                    Reply(request, ResponseTargetType.Private, "Invalid access token request, please consult the help for details.");
                }
            }
        }
    }
}