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
    [Module("MDoc", "Search MonoDoc", "~mdoc <search terms>", AccessLevel.Public)]
    public class MDocSearchModule : SiteSearchModuleBase
    {
        #region Constructors
        public MDocSearchModule()
            : base("MDoc", "mdoc", "www.go-mono.com/docs")
        {
        }
        #endregion
    }
}