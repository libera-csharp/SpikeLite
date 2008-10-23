/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
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
            // Create our application context for Spring.NET.
            IApplicationContext ctx = ContextRegistry.GetContext();

            // Grab our bean and spin it up.
            SpikeLite.SpikeLite bot = (SpikeLite.SpikeLite)ctx.GetObject("SpikeLite");
            bot.Start();

            // Handle SIGTERM gracefully
            Console.CancelKeyPress += delegate
            {
                bot.Shutdown();
                bot.Quit("Caught SIGTERM, quitting");
            };
        }
    }
}