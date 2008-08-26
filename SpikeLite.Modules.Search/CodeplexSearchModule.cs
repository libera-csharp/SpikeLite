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
    [Module("Codeplex", "Search Codeplex", "~codeplex <search terms>", AccessLevel.Public)]
    public class CodeplexSearchModule : SiteSearchModuleBase
    {
        #region Constructors
        public CodeplexSearchModule()
            : base("Codeplex", "codeplex", "www.codeplex.com")
        {
        }
        #endregion
    }
}