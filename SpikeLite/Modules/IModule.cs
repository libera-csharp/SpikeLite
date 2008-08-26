/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using SpikeLite.Communications;
using SpikeLite.Persistence;

namespace SpikeLite.Modules
{
    /// <summary>
    /// This class defines the contract of a given SpikeLite module.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// When the bot receives a message that it deems is worth forwarding to the modules, it passes the message
        /// onto each module allowing it a chance to handle the request.
        /// </summary>
        /// 
        /// <param name="request">A <see cref="Request"/> object containing our message.</param>
        /// 
        /// <remarks>
        /// This method does not currently allow a consumer to mark a message as "handled." Descrimination must be done
        /// on a per-module basis.
        /// </remarks>
        void HandleRequest(Request request);

        /// <summary>
        /// A Crude hack to allow our module manager to inject an instance of itself and some information about
        /// the persistence layer.
        /// </summary>
        /// 
        /// <param name="moduleManager">The module manager itself. This allows chaining of modules (for now).</param>
        /// 
        /// <param name="persistenceLayer">
        /// An instance of the <see cref="PersistenceLayer"/>, allowing access to DAOs as well as individual
        /// entities.
        /// </param>
        void InitModule(ModuleManager moduleManager, PersistenceLayer persistenceLayer);
    }
}
