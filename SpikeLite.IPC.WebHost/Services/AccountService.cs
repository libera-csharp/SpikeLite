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
using AutoMapper;
using SpikeLite.AccessControl;
using SpikeLite.Domain.Model.Authentication;
using System.Linq;
using SpikeLite.Domain.Persistence.Authentication;
using TransportKnownHost = SpikeLite.IPC.WebHost.Transport.KnownHost;
using TransportKnownHostMetaDatum = SpikeLite.IPC.WebHost.Transport.KnownHostMetaDatum;

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

        /// <summary>
        /// Gets a collection of <see cref="Transport.KnownHost"/> corresponding to all the users known to the system.
        /// </summary>
        /// 
        /// <returns>A collection of all the hosts known to the system. If this is empty you might have configuration problems.</returns>
        [OperationContract]
        [SecuredOperation("manageUsers")]
        TransportKnownHost[] GetAllUsers();

        /// <summary>
        /// Gets a user by id.
        /// </summary>
        /// 
        /// <param name="id">The ID of the user to search for.</param>
        /// 
        /// <returns>A <see cref="Transport.KnownHost"/> that corresponds to the ID, if known.</returns>
        [OperationContract]
        [SecuredOperation("manageUsers")]
        TransportKnownHost GetUserById(int id);

        /// <summary>
        /// Revokes an issued access token, using the primary identifier for a <see cref="KnownHost"/>.
        /// </summary>
        /// 
        /// <param name="accessToken">The access token literal to revoke.</param>
        [OperationContract]
        [SecuredOperation("manageUsers")]
        void RevokeAccessToken(string accessToken);

    }

    /// <summary>
    /// Provides a concrete implementation of <see cref="IAccountService"/>.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AccountService : AbstractUserContextAwareService, IAccountService
    {
        public override void Configure()
        {
            Mapper.CreateMap<KnownHost, TransportKnownHost>();
            Mapper.CreateMap<KnownHostMetaDatum, TransportKnownHostMetaDatum>();

            Mapper.AssertConfigurationIsValid();
        }

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

        public TransportKnownHost[] GetAllUsers()
        {
            var knownHostDao = GetBean<IKnownHostDao>("KnownHostDao");
            return Mapper.Map<IList<KnownHost>, TransportKnownHost[]>(knownHostDao.FindAll());
        }

        public TransportKnownHost GetUserById(int id)
        {
            var knownHostDao = GetBean<IKnownHostDao>("KnownHostDao");
            return Mapper.Map<KnownHost, TransportKnownHost>(knownHostDao.FindAll().FirstOrDefault(x => x.Id == id));
        }

        public void RevokeAccessToken(string accessToken)
        {
            var authenticationManager = GetBean<IrcAuthenticationModule>("IrcAuthenticationModule");
            var host = authenticationManager.FindHostByAccessToken(accessToken);

            if (null != host)
            {
                host.AccessTokenExpiration = DateTime.Now.ToUniversalTime();
                authenticationManager.UpdateHost(host);
            }
        }
    }
}
