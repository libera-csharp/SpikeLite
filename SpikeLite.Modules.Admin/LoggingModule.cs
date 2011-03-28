/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using System.IO;
using log4net.Config;
using log4net.Core;
using log4net.Ext.Trace;
using log4net.Repository.Hierarchy;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;
using Cadenza.Collections;

namespace SpikeLite.Modules.Admin
{
    /// <summary>
    /// This class is an administrative module that allows users with <see cref="AccessLevel.Root"/> access to change the logging treshold of all appenders. Valid arguments
    /// are <code>DEBUG</code>, <code>INFO</code> and <code>WARN</code>
    /// </summary>
    /// 
    /// <remarks>
    /// Due to the nature of how TRACE level logging is implemented, we do not currently support dynamically changing to TRACE level logging. To do so please restart the
    /// bot and modify your app.config.
    /// </remarks>
    public class LoggingModule : ModuleBase
    {
        /// <summary>
        /// Holds a logger which we can, amusingly enough, use for logging treshold changes to our loggers.
        /// </summary>
        private readonly TraceLogImpl _logger = (TraceLogImpl) TraceLogManager.GetLogger(typeof (LoggingModule));

        /// <summary>
        /// Holds a mapping of level name -> level entry. We use this because for some reason the Log4Net folks use a class instead of an enum.
        /// </summary>
        private readonly Dictionary<string, Level> _loggingLevels = new Dictionary<string, Level> { {"debug", Level.Debug}, {"info", Level.Info}, {"warn", Level.Warn}};

        public override void HandleRequest(Request request)
        {
            string[] message = request.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (request.RequestFrom.AccessLevel >= AccessLevel.Root
                && message[0].Equals("~logging", StringComparison.OrdinalIgnoreCase)
                && message.Length == 2)
            {
                string levelName = message[1].ToLower();

                if (_loggingLevels.ContainsKey(levelName))
                {
                    Level loggingLevel = _loggingLevels[levelName];

                    LoggerManager.GetAllRepositories().ForEach(x =>
                    {
                        x.Threshold = loggingLevel;
                        x.GetCurrentLoggers().ForEach(y => ((Logger)y).Level = loggingLevel);
                    });

                    ((Hierarchy)log4net.LogManager.GetRepository()).Root.Level = loggingLevel;
                    XmlConfigurator.Configure(new FileInfo("log4net.xml"));

                    // You may not actually catch this if you've just set the level to WARN, but if you're doing that type of filtering chances are you don't care anyway.
                    _logger.InfoFormat("Set logging level to {0}.", loggingLevel.DisplayName);
                }
            }
        }
    }
}
