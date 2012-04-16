/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using log4net.Ext.Trace;
using SpikeLite.Domain.Model.Authentication;
using SpikeLite.Domain.Persistence.Authentication;
using System.Linq;
using Cadenza.Collections;

namespace SpikeLite.AccessControl
{
    /// <summary>
    /// Defines an authentication module that does IRC-based authentication using a set of cloaks or other
    /// user information.
    /// </summary>
    public class IrcAuthenticationModule : IAuthenticationModule
    {
        #region Data Members and Properties

        // TODO: Kog 12/25/2008 - Maybe we can use NHibernate's secondary cache for this on the DAO?

        /// <summary>
        /// Cache the set of cloaks we already know about.
        /// </summary>
        private readonly IList<KnownHost> _cloaks;

        /// <summary>
        /// This stores our known host dao, injected at construction time.
        /// </summary>
        private readonly IKnownHostDao _hostDao;

        /// <summary>
        /// Stores our log4net logger.
        /// </summary>
        private static readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(IrcAuthenticationModule));

        #endregion 

        #region Construction and Associated Helpers

        /// <summary>
        /// Constructs our authentication module and caches all hosts known to the system at spin-up time.
        /// </summary>
        /// 
        /// <param name="hostDao">An implementation of our host dao, injected into our constructor.</param>
        /// <param name="cloaks">A set of hosts to seed, if none are already known.</param>
        public IrcAuthenticationModule(IKnownHostDao hostDao, IEnumerable<KnownHost> cloaks)
        {
            _hostDao = hostDao;
            _cloaks = FindOrSeedAcls(cloaks);
        }

        /// <summary>
        /// A hacktastical convenience method to ensure that we at least have the default set of ACLs. This should be in some sort of installer.
        /// </summary>
        /// 
        /// <returns>The set of all ACLs within the system, or the default set injected set of hosts if the cloaks table is empty.</returns>
        private IList<KnownHost> FindOrSeedAcls(IEnumerable<KnownHost> seedCloaks)
        {
            IList<KnownHost> cloaks = _hostDao.FindAll();

            if (cloaks.Count < 1)
            {                
                _logger.Info("No known hosts found, seeding...");

                seedCloaks.ForEach(cloak => { _hostDao.SaveOrUpdate(cloak); 
                                              _logger.Info(String.Format("Creating host {0}, level {1}, match type {2}", 
                                                                         cloak.HostMask, cloak.AccessLevel, cloak.HostMatchType)); });
                cloaks = _hostDao.FindAll();
            }

            return cloaks;
        }

        #endregion 

        #region Authentication 

        /// <summary>
        /// Attempts to authenticate a given user token.
        /// </summary>
        /// 
        /// <param name="user">A User token passed into us to inspect.</param>
        /// 
        /// <returns>An internal representation of any rights and privileges said user token might possess.</returns>
        public AuthToken Authenticate(IUserToken user)
        {
            return Authenticate(user as IrcUserToken);
        }

        public KnownHost FindHostByCloak(string cloak)
        {
            return FindKnownHostForCloakHostmask(cloak);
        }

        /// <summary>
        /// Attempts to authenticate an IRC-based user token.
        /// </summary>
        /// 
        /// <param name="user">An IRC user token.</param>
        /// 
        /// <returns>An internal representation of any rights and privileges said user token might possess.</returns>
        private AuthToken Authenticate(IrcUserToken user)
        {
            if (user != null)
            {
                KnownHost knownHost = FindKnownHostForCloakHostmask(user.HostMask);
                AccessLevel accessLevel = (knownHost == null) ? AccessLevel.None : knownHost.AccessLevel;

                if (accessLevel != AccessLevel.None)
                {
                    return new AuthToken(this, user, accessLevel);
                }
            }

            return null;
        }

        /// <summary>
        /// Attempts to validate a given hostmask against the set of known hosts.
        /// </summary>
        /// 
        /// <param name="hostMask">A hostmask to check to against our cache.</param>
        /// 
        /// <returns>An associated access level for the mask, which may be <code>NONE</code> if the mask is unknown.</returns>
        private KnownHost FindKnownHostForCloakHostmask(string hostMask)
        {
            AccessLevel userLevel = AccessLevel.None;
            KnownHost targetCloak = null;

            if (!string.IsNullOrEmpty(hostMask))
            {
                foreach (KnownHost cloak in _cloaks)
                {
                    if (userLevel < cloak.AccessLevel && cloak.Matches(hostMask))
                    {
                        userLevel = cloak.AccessLevel;
                        targetCloak = cloak;
                    }
                }
            }

            return targetCloak;
        }

        #endregion 

        #region Stuff that should be refactored into another class

        /**
         * This stuff is hacked in temporarily to deal with our authentication strategy. We really shouldn't have the authentication module knowing about hosts, or
         * caching them - there should be some sort of repository (say, a User Token Cache). But, at present we do, so let's hack these in here and inject this
         * specific module.
         */

        /// <summary>
        /// Adds a <see cref="KnownHost"/> entry to our cache.
        /// </summary>
        /// 
        /// <param name="host">A <see cref="KnownHost"/> to add to our cache.</param>
        public void RememberHost(KnownHost host)
        {
            _cloaks.Add(host);
            _hostDao.SaveOrUpdate(host);
        }

        /// <summary>
        /// Attempts to persist changes to a known host.
        /// </summary>
        /// 
        /// <param name="host">The <see cref="KnownHost"/> to persist changes to.</param>
        public void UpdateHost(KnownHost host)
        {
            _hostDao.SaveOrUpdate(host);    
        }

        /// <summary>
        /// Removes a <see cref="KnownHost"/> entry from our cache by hostmask.
        /// </summary>
        /// 
        /// <param name="hostmask">The literal hostmask to remove.</param>
        /// 
        /// <remarks>
        /// This operation does a literal search on the hostmask, and will have no effect if the match is not exact. We should never have multiple <see cref="KnownHost"/>
        /// entries with the same hostmask.
        /// </remarks>
        public void ForgetHost(string hostmask)
        {
            var host = _cloaks.First(x => x.HostMask.Equals(hostmask, StringComparison.OrdinalIgnoreCase));
            _cloaks.Remove(host);
            _hostDao.Delete(host);
        }

        /// <summary>
        /// Returns the set of hosts known to the system.
        /// </summary>
        /// 
        /// <returns>An <see cref="IEnumerable{T}"/> of known hosts within the system.</returns>
        public IEnumerable<KnownHost> GetHosts()
        {
            return _cloaks.AsEnumerable();
        }

        /// <summary>
        /// A convenience method, this tells us if we know the specific hostmask.
        /// </summary>
        /// 
        /// <param name="hostmask">An exact hostmask to match.</param>
        /// <returns>True if we know the hostmask, else false.</returns>
        public bool KnowsHost(string hostmask)
        {
            return _cloaks.Any(x => x.HostMask.Equals(hostmask, StringComparison.OrdinalIgnoreCase));
        }


        public T ExchangeTokenForConcreteType<T>(IUserToken token) where T : class, IUserToken
        {
            // TODO [Kog 07/23/2011] : Yes, this implementation is absolutely awful. Need to swap around some of the auth APIs.
            return token as T;
        }

        // TODO [Kog 07/25/2011] : This is a stub for now too. This totally doesn't belong on the auth module.

        public KnownHost FindHostByEmailAddress(string emailAddress, string accessToken, Func<DateTime?, bool> longevityFilter)
        {
            return _cloaks.FirstOrDefault(x => !string.IsNullOrEmpty(x.EmailAddress) 
                                               && x.EmailAddress.Equals(emailAddress, StringComparison.OrdinalIgnoreCase)
                                               && x.AccessToken.Equals(accessToken, StringComparison.InvariantCultureIgnoreCase)
                                               && longevityFilter(x.AccessTokenExpiration));
        }

        public KnownHost FindHostByAccessToken(string token)
        {
            return _cloaks.FirstOrDefault(x => !String.IsNullOrEmpty(x.AccessToken) && 
                                               x.AccessToken.Equals(token));
        }

        public KnownHost FindHostById(int id)
        {
            return _cloaks.FirstOrDefault(x => x.Id == id);
        }

        public KnownHost FindHostByHostmaskMatch(string hostmask)
        {
            return _cloaks.FirstOrDefault(x => x.HostMask.Equals(hostmask, StringComparison.InvariantCultureIgnoreCase));
        }

        #endregion 
    }
}
