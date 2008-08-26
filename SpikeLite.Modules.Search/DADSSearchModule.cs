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
    [Module("DADS", "Search Dictionary of Algorithms and Data Structures at NIST", "~dads <search terms>", AccessLevel.Public)]
    public class DADSSearchModule : SiteSearchModuleBase
    {
        #region Constructors
        public DADSSearchModule()
            : base("DADS", "dads", "www.nist.gov/dads/HTML")
        {
        }
        #endregion
    }
}