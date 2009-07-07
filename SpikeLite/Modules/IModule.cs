/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.Communications;
using SpikeLite.Persistence.Authentication;

namespace SpikeLite.Modules
{
    /// <summary>
    /// This class defines the contract of a given SpikeLite module.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// When the bot receives a message that it deems is worth forwarding to the modules, it passes the message
        /// onto each module allowing it a chance to handle the request.
        /// </summary>
        /// 
        /// <param name="request">A <see cref="Request"/> object containing our message.</param>
        /// 
        /// <remarks>
        /// This method does not currently allow a consumer to mark a message as "handled." Descrimination must be done
        /// on a per-module basis.
        /// </remarks>
        void HandleRequest(Request request);

        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the module.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the associated usage instructions of the module.
        /// </summary>
        string Instructions { get; set; }

        /// <summary>
        /// Gets or sets the minimum access requirements for using this module.
        /// </summary>
        AccessLevel RequiredAccessLevel { get; set; }
    }
}
