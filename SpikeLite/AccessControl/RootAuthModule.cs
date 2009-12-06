/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.AccessControl
{
    /// <summary>
    /// Wraps other AuthenticationModules to ensure that the AuthToken returned is not null.
    /// </summary>
    /// 
    /// <remarks>
    /// This should only be used as the top level (root) IAuthenticationModule, as all other
    /// IAuthenticationModule are expected to return null to indicate that they could not authenticate
    /// the request.
    /// 
    /// This IAuthenticationModule ensures that if no IAuthenticationModule can authenticate the request
    /// that an AuthToken with the AccessLevel None is returned so that the callee can always safely
    /// test AuthToken.AccessLevel without having to worry about null values.
    /// </remarks>
    class RootAuthModule : IAuthenticationModule
    {
        public IAuthenticationModule AuthModule { get; set; }

        public AuthToken Authenticate( UserToken user )
        {
            return AuthModule.Authenticate(user) ?? new AuthToken(this, user, AccessLevel.None);
        }

        public KnownHost FindHostByCloak(string cloak)
        {
            return AuthModule.FindHostByCloak(cloak);
        }
    }
}
