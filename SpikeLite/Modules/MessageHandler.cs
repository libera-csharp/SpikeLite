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
    /// A concrete implementation of <see cref="IModuleMessageHandler"/> that we use for logging incoming messages before we MUX them.
    /// </summary>
    public class MessageHandler : IModuleMessageHandler
    {
        public void ConsumeRawMessage(object sender, RequestReceivedEventArgs e)
        {
            ModuleManager.HandleRequest(e.Request);
        }

        public ModuleManager ModuleManager
        {
            get; set;
        }
    }
}
