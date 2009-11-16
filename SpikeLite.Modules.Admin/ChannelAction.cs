/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Modules.Admin
{
    /// <summary>
    /// This administrative command allows us to take actions with channels as a target. Such actions are things like parting and joinin.
    /// </summary>
    public class ChannelAction : ModuleBase
    {
        #region Data Members and Properties

        /// <summary>
        /// Holds a mapping of commands to supported actions. This is wired up in our constructor.
        /// </summary>
        private readonly Dictionary<string, Action<string>> _supportedOperations;

        /// <summary>
        /// Holds a reference to our <see cref="CommunicationManager"/>, which is injected by our IoC container.
        /// </summary>
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private CommunicationManager CommunicationsManagerContext { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        #endregion 

        /// <summary>
        /// Wires up our command object, configures the set of operations we'll respond to.
        /// </summary>
        public ChannelAction()
        {          
            // Wire up our supported operations.
            _supportedOperations = new Dictionary<string, Action<string>> {{"join", x => CommunicationsManagerContext.JoinChannel(x)}, 
                                                                           {"part", x => CommunicationsManagerContext.PartChannel(x)}};
        }

        public override void HandleRequest(Request request)
        {
            // The user has permission and is indeed requesting a shutdown.
            if (request.RequestFrom.AccessLevel >= AccessLevel.Root && request.Message.StartsWith("~Channel", StringComparison.OrdinalIgnoreCase))
            {
                // Parse our message and split out a couple of vars... a tad bit nicer than magic numbers.
                string[] splitMessage = request.Message.Split(' ');
                string operationDescription = splitMessage[1];
                string target = splitMessage[2];

                // Do the operation if we know how, else log.
                if (splitMessage.Length == 3 && _supportedOperations.ContainsKey(operationDescription))
                {
                    var operation = _supportedOperations[operationDescription];
                    operation.Invoke(target);
                }
                else
                {
                    Logger.InfoFormat("Caught a channel action request that we don't know how to parse from {0}: {1}", request.Nick, request.Message);
                }
            }
        }
    }
}
