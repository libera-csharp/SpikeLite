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
    [Module("Pattern", "Search DoFactory.com/Patterns/", "~pattern <search terms>", AccessLevel.Public)]
    public class PatternSearchModule : SiteSearchModuleBase
    {
        #region Constructors
        public PatternSearchModule()
            : base("Pattern", "pattern", "www.dofactory.com/Patterns/")
        {
        }
        #endregion
    }
}