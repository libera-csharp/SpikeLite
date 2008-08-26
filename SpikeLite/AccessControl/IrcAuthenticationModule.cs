/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Collections.Generic;
using System.Text;
using Sharkbite.Irc;

namespace SpikeLite.AccessControl
{
    class IrcAuthenticationModule : AuthenticationModule
    {
        private Cloaks _cloaks;

        public IrcAuthenticationModule(Connection client)
        {
            _cloaks = new Cloaks(client);
        }

        public AuthToken Authenticate( UserToken user )
        {
            return Authenticate(user as IrcUserToken);
        }

        public AuthToken Authenticate( IrcUserToken user )
        {
            if (user == null)
            {
                return null;
            }

            AccessLevel accessLevel = ValidateCloak(user.HostMask);

            if ( accessLevel != AccessLevel.None )
            {
                return new AuthToken(this, user, accessLevel);
            }

            return null;
        }

        private AccessLevel ValidateCloak( string cloakMask )
        {
            if (cloakMask == null || cloakMask.Length == 0)
            {
                return AccessLevel.None;
            }

            AccessLevel userLevel = AccessLevel.None;

            foreach ( Cloak cloak in _cloaks )
            {
                if ( userLevel < cloak.UserLevel && cloak.Matches(cloakMask) )
                {
                    userLevel = cloak.UserLevel;
                }
            }

            return userLevel;
        }
    }
}
