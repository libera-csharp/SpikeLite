/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System.Collections.Generic;
using SpikeLite.Persistence;
using SpikeLite.Persistence.Authentication;

namespace SpikeLite.AccessControl
{
    class IrcAuthenticationModule : AuthenticationModule
    {
        private readonly IEnumerable<KnownHost> _cloaks;
        private readonly KnownHostDao _hostDao;

        public IrcAuthenticationModule(PersistenceLayer persistenceLayer)
        {
            _hostDao = new KnownHostDao(persistenceLayer);
            _cloaks = _hostDao.FindAll();
        }

        public AuthToken Authenticate(UserToken user)
        {
            return Authenticate(user as IrcUserToken);
        }

        public AuthToken Authenticate(IrcUserToken user)
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

        private AccessLevel ValidateCloak(string cloakMask)
        {
            AccessLevel userLevel = AccessLevel.None;

            if (!string.IsNullOrEmpty(cloakMask))
            {
                foreach (KnownHost cloak in _cloaks)
                {
                    if (userLevel < cloak.AccessLevel && cloak.Matches(cloakMask))
                    {
                        userLevel = cloak.AccessLevel;
                    }
                }
            }

            return userLevel;
        }
    }
}
