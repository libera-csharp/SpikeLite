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

namespace SpikeLite.Modules
{
    /// <summary>
    /// This class manages the loading and unloading of modules, as well as connecting a given module to
    /// the communications manager, acting as a dispatcher on messages received.
    /// </summary>
    public class ModuleManager
    {
        #region Properties and Data Members

        // TODO: Kog 12/25/2008 - We can probably get rid of the dispatching via Spring and via either AOP or
        // via injecting our communications manager. Or perhaps using the Spring decoupled events.

        /// <summary>
        /// This stores our communications manager, which we intercept messages from.
        /// </summary>
        protected CommunicationManager _communicationManager;

        /// <summary>
        /// This holds a reference to our Log4Net logger.
        /// </summary>
        private TraceLogImpl _logger;

        /// <summary>
        /// This allows us to inject our list of modules via spring.
        /// </summary>
        public IList<IModule> Modules
        {
            get; set;
        }

        /// <summary>
        /// Gets the Log4NET logger that we're using.
        /// </summary>
        protected virtual TraceLogImpl Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = (TraceLogImpl) TraceLogManager.GetLogger(typeof(ModuleManager));
                }

                return _logger;
            }
        }

        #endregion 

        #region Construction and Module Loading

        // TODO: Kog 12/27/2008 - Why are we constructor injecting this?

        /// <summary>
        /// Spin up our module manager and start intercepting events.
        /// </summary>
        /// 
        /// <param name="communicationManager">Our communications manager.</param>
        public ModuleManager(CommunicationManager communicationManager)
        {
            _communicationManager = communicationManager;
            _communicationManager.RequestReceived += CommunicationManagerRequestReceived;
        }

        public void LoadModules()
        {            
            foreach (ModuleBase module in Modules)
            {
                // TODO: Kog 12/26/2008 - Yeah, the network support is hacked in to only support a single
                // TODO:                  network. We can't support multiples right now anyway, it's a TODO.
                module.NetworkConnectionInformation = _communicationManager.NetworkList[0];
                Logger.InfoFormat("Loaded module {0}", module.Name);
            }
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
            _communicationManager.SendResponse(response);
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
            foreach (ModuleBase module in Modules)
            {
                module.HandleRequest(request);
            }
        }

        /// <summary>
        /// Attempt to handle an incoming message, sending it to all registered modules.
        /// </summary>
        /// 
        /// <param name="sender">The object sending the message, ignored.</param>
        /// <param name="e">Our incoming message arguments.</param>
        void CommunicationManagerRequestReceived(object sender, RequestReceivedEventArgs e)
        {
            HandleRequest(e.Request);
        }

        #endregion
    }
}