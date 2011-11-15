/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.AccessControl
{
    public class AuthToken
    {
        private readonly IAuthenticationModule _issuer;
        private readonly IUserToken _user;
        private readonly AccessLevel _accessLevel;

        public AuthToken(IAuthenticationModule issuer, IUserToken user, AccessLevel accessLevel)
        {
            _issuer = issuer;
            _user = user;
            _accessLevel = accessLevel;
        }

        public IAuthenticationModule Issuer
        {
            get { return _issuer; }
        }

        public IUserToken User
        {
            get { return _user; }
        }

        public AccessLevel AccessLevel
        {
            get { return _accessLevel; }
        }
    }
}
