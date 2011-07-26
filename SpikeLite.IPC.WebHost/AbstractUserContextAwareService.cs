/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.ServiceModel;
using SpikeLite.Domain.Model.Authentication;

// TODO [Kog 07/25/2011] : Come back and clean this up.

namespace SpikeLite.IPC.WebHost
{
    /// <summary>
    /// Provides a service base class that is "user context" aware - namely, it can pull from a predetermined incoming property. Mostly a convenience.
    /// </summary>
    public abstract class AbstractUserContextAwareService
    {
        /// <summary>
        /// Attempts to grab the user principal from the incoming message properties. Assuming that you're decorated with a <see cref="SecuredOperation"/> attribute, this should
        /// be populated for free.
        /// </summary>
        /// 
        /// <returns>A <see cref="KnownHost"/> as principal, if available.</returns>
        /// 
        /// <remarks>
        /// Make sure to use this in conjunction with a <see cref="SecuredOperation"/> attribute.
        /// </remarks>
        public KnownHost GetPrincipal()
        {
            object principal;
            OperationContext.Current.IncomingMessageProperties.TryGetValue("principal", out principal);

            return principal as KnownHost;
        }
    }
}
