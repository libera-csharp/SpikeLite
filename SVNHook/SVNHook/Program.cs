/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using SVNHook.MessagingService;

namespace SVNHook
{
    public class Program
    {
        /// <summary>
        /// Passes commit notices to the bot. There's a post-commit hook that passes in two params (wrapped in quotes): the channel target
        /// and the commit message using SVNLook, that looks something like:  
        /// 
        /// <code>
        /// "R143 [2009-12-09 04:36:11 -0800 (Wed, 09 Dec 2009)] kog - More whitespace. Let's test the commit hook one last time."
        /// </code>
        /// 
        /// This is pretty simple, but it does the job for now.
        /// </summary>
        /// 
        /// <param name="args">An array of strings such that the first argument is a channel target the bot is in, and the second is a commit message.</param>
        /// 
        /// <remarks>
        /// This app was generated using MS' svcutil implementation. The mono implementation didn't seem to want to parse, and both SOAPpy and SOAP::Lite had
        /// problems as well. I've kept the output mostly the same, except we currently don't use authentication/security on the WS endpoints due to Mono's 
        /// lack of implementation. Not surprisingly, Mono can't have any of the security attributes in the app.config, so I stripped them out (not like
        /// we had anything anyway).
        /// </remarks>
        static void Main(string[] args)
        {
            if (args != null && args.Length > 1)
            {
                try
                {
                    string target = args[0];
                    string message = args[1];

                    using (MessagingServiceClient proxy = new MessagingServiceClient())
                    {
                        proxy.SendMessage(target, message);              
                    }
                }
                catch (Exception ex)
                {
                    // Barf if we must. But make sure to fail.
                    Console.WriteLine(ex);
                    Environment.Exit(1);
                }
            }
        }
    }
}
