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
    [Module("Refactoring", "Search Refactoring.com", "~refactoring <search terms>", AccessLevel.Public)]
    public class RefactoringSearchModule : SiteSearchModuleBase
    {
        #region Constructors
        public RefactoringSearchModule()
            : base("Refactoring", "refactoring", "www.refactoring.com/catalog/")
        {
        }
        #endregion
    }
}