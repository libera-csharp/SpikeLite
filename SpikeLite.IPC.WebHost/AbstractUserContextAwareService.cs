/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.ServiceModel;
using SpikeLite.Domain.Model.Authentication;
using Spring.Context.Support;

// TODO [Kog 07/25/2011] : Come back and clean this up.

namespace SpikeLite.IPC.WebHost
{
    /// <summary>
    /// Provides a service base class that is "user context" aware - namely, it can pull from a predetermined incoming property. Mostly a convenience.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public abstract class AbstractUserContextAwareService : IConfigurableServiceHost
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

        /// <summary>
        /// Gets a Spring.NET bean by name.
        /// </summary>
        /// 
        /// <typeparam name="T">The type of the bean.</typeparam>
        /// 
        /// <param name="beanName">The name of the bean to get.</param>
        /// 
        /// <returns>The bean corresponding to the name/type, if available.</returns>
        protected T GetBean<T>(string beanName)
        {
            return (T)ContextRegistry.GetContext().GetObject(beanName, typeof(T));
        }

        /// <summary>
        /// Provides a hook for run-once configuration, such as AutoMapper mappings.
        /// </summary>
        public virtual void Configure()
        {
            // Override me if you want custom configuration.    
        }
    }
}
