/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Collections.Generic;
using Cadenza.Collections;
using log4net.Ext.Trace;
using SpikeLite.Communications;

namespace SpikeLite.Modules
{
    /// <summary>
    /// This class manages the loading and unloading of modules, as well as connecting a given module to
    /// the communications manager, acting as a dispatcher on messages received.
    /// </summary>
    public class ModuleManager
    {
        #region Properties and Data Members

        /// <summary>
        /// Holds our Log4Net trace logger.
        /// </summary>
        private readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(ModuleManager));

        /// <summary>
        /// This allows us to inject our list of modules via spring.
        /// </summary>
        public IEnumerable<IModule> Modules { get; set; }

        /// <summary>
        /// This stores our communications manager, which we intercept messages from.
        /// </summary>
        /// 
        /// <remarks>
        /// This method is internal instead of private for reasons of injection.
        /// </remarks>
        internal CommunicationManager CommunicationManager { get; set; }

        #endregion 

        #region Construction and Module Loading

        // TODO: Kog 11/23/2009 - Refactor to support multiple networks, as needed.
        // TODO: Kog 11/23/2009 - Do we want to refactor this to load all our modules into a single (or even multiple) app domains? This would allow us to do more dynamic loading/unloading.

        /// <summary>
        /// Loads all our modules and sets up our message interception.
        /// </summary>
        public void LoadModules()
        {
            // TODO: Kog 1/13/2010 - Maybe we can just axe the event and swap to something like a iteration over funcs or actions.
            CommunicationManager.RequestReceived += (sender, args) => HandleRequest(args.Request);

            Modules.ForEach(module => { module.NetworkConnectionInformation = CommunicationManager.NetworkList[0]; 
                                        _logger.InfoFormat("Loaded Module {0}", module.Name); });
        }

        #endregion

        #region Message Passing

        /// <summary>
        /// Attempt to send an outgoing message through our communications manager.
        /// </summary>
        /// 
        /// <param name="response">The outgoing message to send.</param>
        public void HandleResponse(Response response)
        {
            CommunicationManager.SendResponse(response);
        }

        // TODO: Kog 11/23/2009 - Do we want to keep the broadcast nature of our commands? Should we, instead, have a 1-1 mapping established at load time
        // TODO: Kog 11/23/2009 - and cached in a dictionary somewhere?

        // TODO: Kog 11/23/2009 - If we need this for the tell module, can we swap to using some sort of advice? Or, can we have the tell module somehow digest
        // TODO: Kog 11/23/2009 - the message, and then ask the module manager to re-handle? Or does that require mutability of the request?

        /// <summary>
        /// Routes an incoming message to all modules for consumption.
        /// </summary>
        /// 
        /// <param name="request">The message to pass.</param>
        /// 
        /// <remarks>
        /// This method may seem useless, but is currently required for our Tell module.
        /// </remarks>
        public void HandleRequest(Request request)
        {
            Modules.ForEach(module => module.HandleRequest(request));
        }

        #endregion
    }
}