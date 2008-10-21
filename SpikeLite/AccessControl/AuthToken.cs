/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.Persistence.Authentication;

namespace SpikeLite.AccessControl
{
    public class AuthToken
    {
        private readonly AuthenticationModule _issuer;
        private readonly UserToken _user;
        private readonly AccessLevel _accessLevel;

        public AuthToken(AuthenticationModule issuer, UserToken user, AccessLevel accessLevel)
        {
            _issuer = issuer;
            _user = user;
            _accessLevel = accessLevel;
        }

        public AuthenticationModule Issuer
        {
            get { return _issuer; }
        }

        public UserToken User
        {
            get { return _user; }
        }

        public AccessLevel AccessLevel
        {
            get { return _accessLevel; }
        }
    }
}
