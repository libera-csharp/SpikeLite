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
    /// This interface forms the contract for anything that can be plugged in as a search provider
    /// for a given search-based module.
    /// </summary>
    public interface ISearchProvider
    {
        /// <summary>
        /// Attempts to execute a search against internally held proxy. See the remarks section
        /// for details.
        /// </summary>
        /// 
        /// <param name="searchCriteria">The search criteria to search for.</param>
        /// <param name="callbackHandler">A tuple containing our request and our callback handler.</param>
        /// 
        /// <remarks>
        /// Since our search handlers are actually asynchronous we accept a delegate from our caller
        /// to make sure that we hand control back to our caller. This also allows things like 
        /// post-search processing and filtering. The request is passed back to the delegate
        /// to allow for post operations.
        /// </remarks>
        void ExecuteSearch(string searchCriteria, string domain, Action<string[]> callbackHandler);
    }
}
