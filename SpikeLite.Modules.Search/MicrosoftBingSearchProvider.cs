/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Bing;

//https://api.datamarket.azure.com/Data.ashx/Bing/Search/v1/Web?Query=%27string%20class%20site%3amsdn.microsoft.com%27&$top=50&$format=Atom 

namespace SpikeLite.Modules.Search
{
    /// <summary>
    /// Provides a Microsft Bing based search provider. 
    /// </summary>
    /// 
    /// <remarks>
    /// The Google search API seems to be either deprecated, or keys are no longer being handed out. Regardless, The number of searches
    /// allowed per Google was less than Live, and Live seems to do a decent enough job (and, indeed powers MSDN itself). Given the higher
    /// allowance of searches, this is the preffered provider at the moment.
    /// </remarks>
    public class MicrosoftBingSearchProvider : AbstractApiKeySearchProvider
    {
        public override void ExecuteSearch(string searchCriteria, string domain, Action<string[]> callbackHandler)
        {
            var _searchBroker = new BingSearchContainer(new Uri("https://api.datamarket.azure.com/Bing/Search/"))
            {
                Credentials = new NetworkCredential(ApiKey, ApiKey)
            };

            var domainSpecificSearchCriteria = searchCriteria.Trim();

            // Not everything needs a domain qualifier - BING and Google being the primary cases. Only add the domain if required, this will
            // prevent BING from searching MS docs no one cares about.
            if (!String.IsNullOrWhiteSpace(domain))
            {
                domainSpecificSearchCriteria += String.Format(" site:{0}", domain);
            }

            //TODO: AJ: Thread
            var query = _searchBroker.Web(domainSpecificSearchCriteria, null, null, "en-US", "Off", null, null, null);
            var webResults = query.Execute();

            var results = new List<string>();

            foreach (var webResult in webResults.Take(1))
            {
                results.Add(String.Format("'%query%': {0} | {1}",
                          webResult.Description,
                          webResult.Url));
            }

            callbackHandler(results.ToArray());
        }
    }
}