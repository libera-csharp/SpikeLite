/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
 
using System;

namespace SpikeLite.Modules.Search
{
    /// <summary>
    /// An abstract class for our search providers, allowing for some wiring and simple re-use.
    /// </summary>
    public abstract class AbstractSearchProvider : ISearchProvider
    {
        public string ApiKey
        {
            get; set;
        }

        public abstract void ExecuteSearch(string searchCriteria, string domain, Action<string[]> callbackHandler);
    }
}
