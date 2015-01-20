/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Threading;
using log4net.Ext.Trace;
using Spring.Context.Support;

namespace SpikeLite.UI.Cli
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
            _logger.Info("Starting console runner");

            try
            {
                // We may actually not log to the console past this point, so let's go ahead and spam something
                // here just in case.
                Console.WriteLine("We've spun up the bot and are currently logging to our appenders. Hit CTL+C to quit.");

                // We want to know when it's ok to shutdown the bot.
                using (var shutdownManualResetEvent = new ManualResetEvent(false))
                {
                    // Get our application context from Spring.NET.
                    var applicationContext = ContextRegistry.GetContext();

                    // Grab our bean and spin it up.
                    var bot = applicationContext.GetObject("SpikeLite") as SpikeLite;

                    // Listen for status changes so we know when to exit
                    bot.BotStatusChanged += (sender, eventArgs) =>
                    {
                        if (eventArgs.NewStatus == BotStatus.Stopped)
                        {
                            _logger.Info("Bot status set to stopped, shutting down.");
                            shutdownManualResetEvent.Set();    
                        }    
                    };

                    // This won't actually work while we're debugging:
                    // http://connect.microsoft.com/VisualStudio/feedback/details/524889/debugging-c-console-application-that-handles-console-cancelkeypress-is-broken-in-net-4-0
                    // Handle SIGTERM gracefully.
                    Console.CancelKeyPress += ((sender, args) =>
                    {
                        _logger.Info("Cancel key pressed. Shutting down bot.");
                        args.Cancel = true;

                        // Clean up.
                        bot.Shutdown("Caught SIGTERM, quitting");                        

                        // Signal that we're ready to shutdown the bot.
                        shutdownManualResetEvent.Set();
                    });

                    bot.Start();

                    // Wait untill we're ready to shutdown the bot.
                    shutdownManualResetEvent.WaitOne();

                    _logger.Info("Application shutting down.");

                    applicationContext.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Fatal error attempting to start the bot: {0} - {1}", ex.Message, ex.StackTrace);
            }
        }
    }
}