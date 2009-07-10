/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using SpikeLite.Communications;

namespace SpikeLite.Modules
{
    /// <summary>
    /// Provides a contract for a class that can handle module messages. This interface exists mainly for the purposes of proxying.
    /// </summary>
    public interface IModuleMessageHandler
    {
        /// <summary>
        /// Consumes a raw incoming request.
        /// </summary>
        /// 
        /// <param name="sender">The object calling us.</param>
        /// <param name="e">Event args.</param>
        void ConsumeRawMessage(object sender, RequestReceivedEventArgs e);

        /// <summary>
        /// Gets or sets a module manager we can talk to.
        /// </summary>
        ModuleManager ModuleManager { get; set; }
    }
}
