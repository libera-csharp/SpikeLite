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
    [Module("Wikipedia", "Search Wikipedia", "~wikipedia <search terms>", AccessLevel.Public)]
    public class WikipediaSearchModule : SiteSearchModuleBase
    {
        #region Constructors
        public WikipediaSearchModule()
            : base("Wikipedia", "wikipedia", "en.wikipedia.org")
        {
        }
        #endregion
    }
}