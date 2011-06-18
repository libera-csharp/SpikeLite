/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using log4net.Ext.Trace;
using Spring.Context;
using Spring.Context.Support;

namespace FrontEnd_Console
{
    /// <summary>
    /// A CLI runner for Spike Lite. 
    /// </summary>
    internal class ConsoleRunner
    {
        /// <summary>
        /// Stores our log4net logger.
        /// </summary>
        private static readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(ConsoleRunner));

        /// <summary>
        /// Does all the magic of spinning up the bot.
        /// </summary>
        private static void Main()
        {
            try
            {
                 // Get our application context from Spring.NET.
                IApplicationContext ctx = ContextRegistry.GetContext();

                // Grab our bean and spin it up.
                SpikeLite.SpikeLite bot = ctx.GetObject("SpikeLite") as SpikeLite.SpikeLite;
                bot.Start();

                // We may actually not log to the console past this point, so let's go ahead and spam something
                // here just in case.
                Console.WriteLine(Environment.NewLine + "We've spun up the bot and are currently logging to our appenders. Hit CTL+C to quit.");

                // Handle SIGTERM gracefully. Tell the bot to shutdown, dispose the context.
                Console.CancelKeyPress += ((sender, args) => { bot.Shutdown("Caught SIGTERM, quitting"); 
                                                               ctx.Dispose(); });
            }
            catch(Exception ex)
            {
                _logger.ErrorFormat("Fatal error attempting to start the bot: {0} - {1}", ex.Message, ex.StackTrace);      
            }
        }
    }
}