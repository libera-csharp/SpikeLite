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
    [Module("pInvoke", "Search pInvoke.Net", "~pinvoke <search terms>", AccessLevel.Public)]
    public class PInvokeSearchModule : SiteSearchModuleBase
    {
        #region Constructors
        public PInvokeSearchModule()
            : base("pInvoke", "pinvoke", "pinvoke.net")
        {
        }
        #endregion
    }
}