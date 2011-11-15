/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

namespace SpikeLite.Shared.Communications
{
    public interface ICommunicationManager
    {
        BotStatus BotStatus { get; }
    }
}