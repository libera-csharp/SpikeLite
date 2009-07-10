/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System.Collections.Generic;
using log4net.Ext.Trace;
using SpikeLite.Communications;
using Mono.Rocks;

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
        public IList<IModule> Modules { get; set; }

        /// <summary>
        /// This stores our communications manager, which we intercept messages from.
        /// </summary>
        protected CommunicationManager CommunicationManager { get; set; }

        #endregion 

        #region Construction and Module Loading

        // TODO: Kog 12/26/2008 - Yeah, the network support is hacked in to only support a single network. We can't support multiples right 
        // TODO:                  now anyway, it's a TODO.

        /// <summary>
        /// Loads all our modules and sets up our message interception.
        /// </summary>
        public void LoadModules()
        {
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

        // TODO: Kog 12/27/2008 - Can we refactor the Tell module not to need this?

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
            //TODO AJ:Add Chain of Responsability for command modules to allow for better message handling
            //TODO AJ:Once CoR has been added the include a dictionary of the first level of modules to make lookup faster
            //TODO AJ:and stop messages being passed to modules that aren't interested.
            Modules.ForEach(module => module.HandleRequest(request));
        }

        #endregion
    }
}