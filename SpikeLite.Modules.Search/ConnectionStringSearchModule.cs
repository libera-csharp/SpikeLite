/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.AccessControl;

namespace SpikeLite.Modules.Search
{
    [Module("Connection Strings", "Search ConnectionString.com", "~connectionstring <search terms>", AccessLevel.Public)]
    public class ConnectionStringSearchModule : SiteSearchModuleBase
    {
        #region Constructors
        public ConnectionStringSearchModule()
            : base("Connection Strings", "connectionstring", "www.connectionstrings.com")
        {
        }
        #endregion
    }
}