/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SpikeLite.Communications;
using SpikeLite.Persistence;
using log4net;

namespace SpikeLite.Modules
{
    /// <summary>
    /// This class manages the loading and unloading of modules, as well as connecting a given module to
    /// the communications manager, acting as a dispatcher on messages received.
    /// </summary>
    public class ModuleManager
    {
        #region Data Members

        // TODO: Kog 12/25/2008 - We can probably get rid of the dispatching via Spring and via either AOP or
        // via injecting our communications manager. Or perhaps using the Spring decoupled events.

        /// <summary>
        /// This stores our communications manager, which we intercept messages from.
        /// </summary>
        protected CommunicationManager _communicationManager;

        /// <summary>
        /// Maps the set of modules by name. This allows us to be able to pull out modules in request chaining.
        /// </summary>
        private readonly Dictionary<string, ModuleBase> _modulesByName = new Dictionary<string, ModuleBase>();

        // TODO: Kog 12/25/2008 - this can probably be done at the IoC container level...

        /// <summary>
        /// Maps the set of module attributes by name. This allows for easy lookup in our help module.
        /// </summary>
        private readonly Dictionary<string, ModuleAttribute> _moduleAttributesByName = new Dictionary<string, ModuleAttribute>();

        /// <summary>
        /// This holds a reference to our Log4Net logger.
        /// </summary>
        private ILog _logger;

        #endregion 

        #region Properties

        // TODO: Kog 12/25/2008 - Get rid of this ASAP.

        /// <summary>
        /// Passes a dictionary of our modules and their names to a given caller.
        /// </summary>
        public Dictionary<string, ModuleBase> ModulesByName
        {
            get { return _modulesByName; }
        }

        // TODO: Kog 12/25/2008 - Get rid of this ASAP.

        /// <summary>
        /// Passes a dictionary of our module attributes and their module names to a given caller.
        /// </summary>
        public Dictionary<string, ModuleAttribute> ModuleAttributesByName
        {
            get { return _moduleAttributesByName; }
        }

        /// <summary>
        /// This allows us to inject our list of modules via spring.
        /// </summary>
        public IList<IModule> Modules
        {
            get; 
            set;
        }

        /// <summary>
        /// Gets the Log4NET logger that we're using.
        /// </summary>
        protected virtual ILog Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = LogManager.GetLogger(typeof(ModuleManager));
                }

                return _logger;
            }
        }

        #endregion 

        /// <summary>
        /// Spin up our module manager and start intercepting events.
        /// </summary>
        /// 
        /// <param name="communicationManager">Our communication manager.</param>
        public ModuleManager(CommunicationManager communicationManager)
        {
            _communicationManager = communicationManager;
            _communicationManager.RequestReceived += communicationManager_RequestReceived;
        }

        #region Methods

        #region LoadModules

        public void LoadModules()
        {
            // TODO: Kog 12/25/2008 - if it weren't already 0700 in the morning I'd fix this... get to it next commit.
            foreach (ModuleBase module in Modules)
            {
                module.ModuleManagementContainer = this;
                ModuleAttribute[] moduleAttributes = (ModuleAttribute[])(module.GetType()).GetCustomAttributes(typeof(ModuleAttribute), false);

                _modulesByName.Add(moduleAttributes[0].Name.ToLower(), module);
                _moduleAttributesByName.Add(moduleAttributes[0].Name.ToLower(), moduleAttributes[0]);

                // TODO: Kog 12/26/2008 - Yeah, the network support is hacked in to only support a single
                // TODO:                  network. We can't support multiples right now anyway, it's a TODO.
                module.NetworkConnectionInformation = _communicationManager.NetworkList[0];

                Logger.Info(moduleAttributes[0].Name);
            }
        }

        #endregion

        public void HandleRequest(Request request)
        {
            //TODO AJ:Add Chain of Responsability for command modules to allow for better message handling
            //TODO AJ:Once CoR has been added the include a dictionary of the first level of modules to make lookup faster
            //TODO AJ:and stop messages being passed to modules that aren't interested.

            //AJ: Forward the Request to each Module
            foreach (ModuleBase module in _modulesByName.Values)
            {
                module.HandleRequest(request);
            }
        }

        public void HandleResponse(Response response)
        {
            _communicationManager.SendResponse(response);
        }

        void communicationManager_RequestReceived(object sender, RequestReceivedEventArgs e)
        {
            HandleRequest(e.Request);
        }

        #endregion
    }
}