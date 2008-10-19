/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.Persistence.Authentication;

namespace SpikeLite.Modules.Search
{
    [Module("MSConnect", "Search Microsoft Connect", "~msconnect <search terms>", AccessLevel.Public)]
    public class ConnectSearchModule : SiteSearchModuleBase
    {
        #region Constructors
        public ConnectSearchModule()
            : base("MSConnect", "connect", "connect.microsoft.com")
        {
        }
        #endregion
    }
}