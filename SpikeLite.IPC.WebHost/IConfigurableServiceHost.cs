/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2012 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

namespace SpikeLite.IPC.WebHost
{
    /// <summary>
    /// Provides a contract that allows us to configure our service hosts in <see cref="ServiceHostModule{I,C}"/>.
    /// </summary>
    public interface IConfigurableServiceHost
    {
        /// <summary>
        /// Provides a callback for configuring the service. This should be minor stuff, like AutoMapper configs. This will be called at most once.
        /// </summary>
        void Configure();
    }
}
