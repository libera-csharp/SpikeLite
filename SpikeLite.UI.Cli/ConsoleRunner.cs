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
            _logger.Trace("Starting console runner");

            try
            {
                // We may actually not log to the console past this point, so let's go ahead and spam something
                // here just in case.
                Console.WriteLine("We've spun up the bot and are currently logging to our appenders. Hit CTL+C to quit.");

                // We want to know when it's ok to shutdown the bot.
                using (var shutdownManualResetEvent = new ManualResetEventSlim(false))
                {
                    // This won't actually work while we're debugging:
                    // http://connect.microsoft.com/VisualStudio/feedback/details/524889/debugging-c-console-application-that-handles-console-cancelkeypress-is-broken-in-net-4-0
                    // Handle SIGTERM gracefully.
                    Console.CancelKeyPress += ((sender, args) =>
                    {
                        // Let us handle the shutdown of the bot
                        args.Cancel = true;

                        // Signal that we're ready to shutdown the bot.
                        shutdownManualResetEvent.Set();

                        _logger.Trace("Cancel key pressed. Shutting down bot.");
                    });

                    // We want to know when it's ok to exit the application.
                    using (var exitManualResetEvent = new ManualResetEventSlim())
                    {
                        // The bot is in a seperate thread so we can block this one until we're ready to exit
                        var botThread = new Thread(() =>
                        {
                            _logger.Trace("Bot thread started.");

                            // Get our application context from Spring.NET.
                            var applicationContext = ContextRegistry.GetContext();

                            // Grab our bean and spin it up.
                            var bot = applicationContext.GetObject("SpikeLite") as SpikeLite;

                            // Listen for status changes so we know when to exit
                            bot.BotStatusChanged += (sender, eventArgs) => 
                            {
                                if(eventArgs.NewStatus == BotStatus.Stopped)
                                    // Signal that we're ready to shutdown the bot.
                                    shutdownManualResetEvent.Set();
                            };

                            bot.Start();

                            // Wait untill we're ready to shutdown the bot.
                            shutdownManualResetEvent.Wait();

                            // Clean up.
                            bot.Shutdown("Caught SIGTERM, quitting");

                            applicationContext.Dispose();

                            _logger.Trace("Bot thread shutdown.");

                            // Signal that we're ready to exit the application.
                            exitManualResetEvent.Set();
                        });

                        botThread.IsBackground = true;
                        botThread.Start();

                        // Wait untill we're ready to exit the application.
                        exitManualResetEvent.Wait();

                        _logger.Trace("Application shutdown.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Fatal error attempting to start the bot: {0} - {1}", ex.Message, ex.StackTrace);
            }
        }
    }
}