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

        /// <summary>
        /// Gets the set of <see cref="AccessFlag"/> known to the system.
        /// </summary>
        /// 
        /// <returns>An array of all the known <see cref="AccessFlag"/> within the system.</returns>
        [OperationContract]
        [SecuredOperation]
        AccessFlag[] GetAllKnownAccessFlags();

        /// <summary>
        /// Creates a new access flag within the system.
        /// </summary>
        /// 
        /// <param name="name">The name of the access flag. Should be an alpha string.</param>
        /// <param name="description">A textual description of the access flag.</param>
        [OperationContract]
        [SecuredOperation("manageUsers")]
        void CreateAccessFlag(string name, string description);

        /// <summary>
        /// Adds a specific flag to a user.
        /// </summary>
        /// 
        /// <param name="userId">The ID of the user to add the flag to.</param>
        /// <param name="accessFlag">The IDs of the flag to add to the given user.</param>
        [OperationContract]
        [SecuredOperation("manageUsers")]
        void AddAccessFlagToUser(int userId, int accessFlag);

        /// <summary>
        /// Revokes an access flag from a user.
        /// </summary>
        /// 
        /// <param name="userId">The ID of the user to revoke the flag from.</param>
        /// <param name="accessFlag">The access flag to revoke.</param>
        [OperationContract]
        [SecuredOperation("manageUsers")]
        void RevokeAccessFlagFromUser(int userId, int accessFlag);
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
            var authenticationManager = GetBean<IrcAuthenticationModule>("IrcAuthenticationModule");
            return Mapper.Map<IEnumerable<KnownHost>, TransportKnownHost[]>(authenticationManager.GetHosts());
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

        public AccessFlag[] GetAllKnownAccessFlags()
        {
            var accessFlagDao = GetBean<IAccessFlagDao>("AccessFlagDao");
            return accessFlagDao.FindAll().ToArray();
        }

        public void CreateAccessFlag(string name, string description)
        {
            var accessFlagDao = GetBean<IAccessFlagDao>("AccessFlagDao");

            if (!accessFlagDao.FindAll().Any(x => x.Flag.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            {
                accessFlagDao.SaveOrUpdate(accessFlagDao.CreateFlag(name, description));
            }
        }

        public void AddAccessFlagToUser(int userId, int accessFlag)
        {
            var authenticationManager = GetBean<IrcAuthenticationModule>("IrcAuthenticationModule");
            var accessFlagDao = GetBean<IAccessFlagDao>("AccessFlagDao");
            var knownHost = authenticationManager.FindHostById(userId);

            // Make sure we know the host corresponding to the id.
            if (null != knownHost)
            {
                // Make sure the access flag actually exists as well...
                var flag = accessFlagDao.FindAll().FirstOrDefault(x => x.Id == accessFlag);

                // Lastly, make sure the flag is not already set.
                if (null != flag && ! knownHost.AccessFlags.Any(x => x.Id == accessFlag))
                {
                    knownHost.AccessFlags.Add(flag);
                    authenticationManager.UpdateHost(knownHost);    
                }                
            }            
        }

        public void RevokeAccessFlagFromUser(int userId, int accessFlag)
        {
            var authenticationManager = GetBean<IrcAuthenticationModule>("IrcAuthenticationModule");
            var accessFlagDao = GetBean<IAccessFlagDao>("AccessFlagDao");
            var knownHost = authenticationManager.FindHostById(userId);

            // Make sure we know the host corresponding to the id.
            if (null != knownHost)
            {
                // Make sure that we know what this flag is.
                if (accessFlagDao.FindAll().Any(x => x.Id == accessFlag))
                {
                    // Try and remove the flag - if it's not set, we should have a no-op here.
                    knownHost.AccessFlags.Remove(knownHost.AccessFlags.FirstOrDefault(x => x.Id == accessFlag));
                    authenticationManager.UpdateHost(knownHost);
                }
            } 
        }
    }
}
