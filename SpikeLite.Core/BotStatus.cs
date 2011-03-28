/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

// TODO: Kog 11/15/2009 - This needs to go, especially if we want multiple network support.

namespace SpikeLite
{
    /// <summary>
    /// Returns the current connection status of the bot.
    /// </summary>
    public enum BotStatus
    {
        /// <summary>
        /// The bot has just started up, and is most likely initializing stuff.
        /// </summary>
        Starting,

        /// <summary>
        /// The bot has finished initializing and is connected. 
        /// </summary>
        Started,

        /// <summary>
        /// The bot is disconnecting. 
        /// </summary>
        Stopping,

        /// <summary>
        /// The bot has finished stopping.
        /// </summary>
        Stopped
    }
}
