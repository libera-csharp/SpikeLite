/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using log4net.Ext.Trace;
using SpikeLite.Communications;
using SpikeLite.Modules;

namespace SpikeLite
{
    /// <summary>
    /// The bot engine, just call "start" and you're off.
    /// </summary>
    public class SpikeLite
    {
        #region Data Members

        /// <summary>
        /// Stores the bot's connection status. 
        /// </summary>
        private BotStatus _botStatus = BotStatus.Stopped;

        /// <summary>
        /// This holds the instace of the logger we use for spamming whatever appender we so desire.
        /// </summary>
        private readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(SpikeLite));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the communications manager we're using. This is usually injected via our IoC container. This
        /// must not be null.
        /// </summary>
        public CommunicationManager CommunicationManager { get; set; }

        /// <summary>
        /// Gets or sets our module manager we're using. This is usually injected via our IoC container. This
        /// must not be null.
        /// </summary>
        public ModuleManager ModuleManager { get; set; }

        /// <summary>
        /// Gets the status of the bot. This may not be set or updated externally to the main engine.
        /// </summary>
        public BotStatus BotStatus
        {
            get { return _botStatus; }
        }

        #endregion

        #region Public Behavior

        /// <summary>
        /// Attempt to spin up all our subsystems and connect to any networks configured.
        /// </summary>
        public void Start()
        {
            _logger.Trace("Start called...");

            // Make sure we're not trying to double start.
            if (_botStatus != BotStatus.Stopped)
            {
                throw new Exception(string.Format("Current BotStatus is : '{0}'. To start the bot the BotStatus must be 'Stopped'.", _botStatus));
            }

            _botStatus = BotStatus.Starting;

            // Spin up our subsystems.
            _logger.Trace("Attempting to load all modules...");
            ModuleManager.LoadModules();

            _logger.Trace("Connecting to networks...");
            CommunicationManager.Connect();

            // Alright, we're cooking now.
            _botStatus = BotStatus.Started;
            _logger.Trace("Start completed.");
        }


        /// <summary>
        /// Kill all our modules, stop all our IPC.
        /// </summary>    
        /// 
        /// <param name="shutdownMessage">An optional shutdown message to be passed to any IRCDs we may be connected to.</param>
        public void Shutdown(string shutdownMessage)
        {
            _logger.InfoFormat("Shutdown: {0}.", shutdownMessage);
            
            _botStatus = BotStatus.Stopped;
            CommunicationManager.Quit(shutdownMessage);

            _logger.Info("Shutdown completed.");
        }
 
        #endregion 
    }
}