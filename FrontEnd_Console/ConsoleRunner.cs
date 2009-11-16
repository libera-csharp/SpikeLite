/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
 
using System;
using Spring.Context;
using Spring.Context.Support;

namespace FrontEnd_Console
{
    /// <summary>
    /// A CLI runner for Spike Lite. 
    /// </summary>
    internal class ConsoleRunner
    {
        private static void Main()
        {
            // Get our application context from Spring.NET.
            IApplicationContext ctx = ContextRegistry.GetContext();

            // Grab our bean and spin it up.
            SpikeLite.SpikeLite bot = ctx.GetObject("SpikeLite") as SpikeLite.SpikeLite;
            bot.Start();

            // We may actually not log to the console past this point, so let's go ahead and spam something
            // here just in case.
            Console.WriteLine(Environment.NewLine + "We've spun up the bot and are currently logging to our appenders. Hit CTL+C to quit.");

            // Handle SIGTERM gracefully.
            Console.CancelKeyPress += ((sender, args) => { bot.Shutdown(); bot.Quit("Caught SIGTERM, quitting"); });
        }
    }
}