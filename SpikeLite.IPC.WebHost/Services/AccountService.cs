/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2012 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using System.ServiceModel;
using SpikeLite.AccessControl;
using SpikeLite.Domain.Model.Authentication;
using System.Linq;

namespace SpikeLite.IPC.WebHost.Services
{
    /// <summary>
    /// Provides a service that allows you to interact with accounts.
    /// </summary>
    [ServiceContract(Namespace = "http://tempuri.org")]
    public interface IAccountService : IConfigurableServiceHost
    {
        /// <summary>
        /// Given the header information, attempts to return the set of flags for a given user.
        /// </summary>
        /// 
        /// <returns>A collection of flags the user has been granted. May be empty, or null.</returns>
        [OperationContract]
        [SecuredOperation]
        AccessFlag[] GetFlags();

        /// <summary>
        /// Generates a new token and associates it. Will be valid for the TTL specified, in days.
        /// </summary>
        /// 
        /// <param name="ttlInDays">The longevity of the token, in days.</param>
        /// 
        /// <returns>An access token.</returns>
        [OperationContract]
        [SecuredOperation]
        string GenerateNewToken(int ttlInDays);

        /// <summary>
        /// Gets the expiration of the token information passed in the headers.
        /// </summary>
        /// 
        /// <returns>A date literal telling the user when their token will expire.</returns>
        [OperationContract]
        [SecuredOperation]
        DateTime GetTokenExpiration();
    }

    /// <summary>
    /// Provides a concrete implementation of <see cref="IAccountService"/>.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AccountService : AbstractUserContextAwareService, IAccountService
    {
        public AccessFlag[] GetFlags()
        {
            var principal = GetPrincipal();
            return null == principal ? new List<AccessFlag>().ToArray() : principal.AccessFlags.ToArray();
        }

        public string GenerateNewToken(int ttlInDays)
        {
            var principal = GetPrincipal();
            var tokenLiteral = String.Empty;

            if (null != principal)
            {
                var authenticationManager = GetBean<IrcAuthenticationModule>("IrcAuthenticationModule");

                // Compute our expiration.
                var accessTokenExpiration = ttlInDays == 0 ? DateTime.MaxValue : DateTime.Now.ToUniversalTime().AddDays(ttlInDays);

                // Generate a new GUID.
                var accessToken = Guid.NewGuid();
                var accessTokenCreationTime = DateTime.Now.ToUniversalTime();
                tokenLiteral = accessToken.ToString();

                // Modify our record.
                principal.AccessToken = tokenLiteral;
                principal.AccessTokenIssueTime = accessTokenCreationTime;
                principal.AccessTokenExpiration = accessTokenExpiration;

                authenticationManager.UpdateHost(principal);
            }

            return tokenLiteral;
        }

        public DateTime GetTokenExpiration()
        {
            var principal = GetPrincipal();
            return principal.AccessTokenExpiration.GetValueOrDefault();
        }
    }
}
