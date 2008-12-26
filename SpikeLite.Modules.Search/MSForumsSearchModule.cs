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
    [Module("MSForums", "Search the Microsoft Forums", "~msforums <search terms>", AccessLevel.Public)]
    public class MSForumsSearchModule : SiteSearchModuleBase
    {
        public MSForumsSearchModule() : base("MSForums", "msforums", "forums.microsoft.com") { }
    }
}