/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using SpikeLite.Domain.Model.Authentication;
using SpikeLite.Domain.Persistence.Authentication;
using System.Linq;

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

        #endregion 

        #region Construction and Associated Helpers

        /// <summary>
        /// Constructs our authentication module and caches all hosts known to the system at spin-up time.
        /// </summary>
        /// 
        /// <param name="hostDao">An implementation of our host dao, injected into our constructor.</param>
        public IrcAuthenticationModule(IKnownHostDao hostDao)
        {
            _hostDao = hostDao;
            _cloaks = FindOrSeedAcls();
        }

        /// <summary>
        /// A hacktastical convenience method to ensure that we at least have the default set of ACLs. This should be in some sort of installer.
        /// </summary>
        /// 
        /// <returns>The set of all ACLs within the system, or the default set from <see cref="KnownHostDao.SeedAcLs"/> if the cloaks table is empty.</returns>
        private IList<KnownHost> FindOrSeedAcls()
        {
            IList<KnownHost> cloaks = _hostDao.FindAll();

            if (cloaks.Count < 1)
            {
                _hostDao.SeedAcLs();
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
        public AuthToken Authenticate(UserToken user)
        {
            return Authenticate(user as IrcUserToken);
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
                AccessLevel accessLevel = ValidateCloak(user.HostMask);

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
        private AccessLevel ValidateCloak(string hostMask)
        {
            AccessLevel userLevel = AccessLevel.None;

            if (!string.IsNullOrEmpty(hostMask))
            {
                foreach (KnownHost cloak in _cloaks)
                {
                    if (userLevel < cloak.AccessLevel && cloak.Matches(hostMask))
                    {
                        userLevel = cloak.AccessLevel;
                    }
                }
            }

            return userLevel;
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
            _hostDao.Save(host);
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
            KnownHost host = _cloaks.First(x => x.HostMask.Equals(hostmask, StringComparison.OrdinalIgnoreCase));
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

        #endregion 
    }
}
