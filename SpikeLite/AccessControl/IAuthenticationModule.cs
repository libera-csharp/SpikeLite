/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
namespace SpikeLite.AccessControl
{
    /// <summary>
    /// Defines the public contract for a given authentication provider.
    /// </summary>
    public interface IAuthenticationModule
    {
        /// <summary>
        /// Attempts to resolve an anonymous set of user information into a user within the system.
        /// </summary>
        /// 
        /// <param name="user">A <see cref="UserToken"/> representing a message source.</param>
        /// 
        /// <returns>An <see cref="AuthToken"/> for the user information.</returns>
        AuthToken Authenticate(UserToken user);
    }
}
