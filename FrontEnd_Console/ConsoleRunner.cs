/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace FrontEnd_Console
{
    /// <summary>
    /// A CLI runner for Spike Lite. 
    /// </summary>
    internal class ConsoleRunner
    {
        private static void Main(string[] args)
        {
            SpikeLite.SpikeLite spikeLite = new SpikeLite.SpikeLite();
            spikeLite.Start();

            // Handle SIGTERM gracefully
            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
            {
                spikeLite.Shutdown();
                spikeLite.Quit("Caught SIGTERM, quitting");
            };
        }
    }
}