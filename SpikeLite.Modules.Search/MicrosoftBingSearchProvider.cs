/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using SpikeLite.Modules.Search.net.bing.api;

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
    public class MicrosoftBingSearchProvider : AbstractSearchProvider
    {
        private readonly BingService _searchBroker;

        public MicrosoftBingSearchProvider()
        {
            _searchBroker = new BingService();
            _searchBroker.SearchCompleted += SearchCompleted;
        }

        public override void ExecuteSearch(string searchCriteria, string domain, Action<string[]> callbackHandler)
        {
            string domainSpecificSearchCriteria = searchCriteria.Trim();

            // Not everything needs a domain qualifier - BING and Google being the primary cases. Only add the domain if required, this will
            // prevent BING from searching MS docs no one cares about.
            if (!String.IsNullOrEmpty(domain))
            {
                domainSpecificSearchCriteria += String.Format(" site:{0}", domain);
            }

            SearchRequest searchRequest = new SearchRequest()
            {
                Adult = AdultOption.Off,
                AppId = ApiKey,
                Market = "en-US",
                Query = domainSpecificSearchCriteria,
                Sources = new SourceType[] { SourceType.Web },
                Version = "2.2",
                Web = new WebRequest() { Count = 1 }
            };

            _searchBroker.SearchAsync(searchRequest, callbackHandler);
        }

        static void SearchCompleted(object sender, SearchCompletedEventArgs e)
        {
            List<string> results = new List<string>();

            // Our Async finder may have thrown an exception, so let's catch it and spit it out at the user if we must.
            try
            {
                if (e.Result.Web.Results.Length > 0)
                {
                    WebResult webResult = e.Result.Web.Results[0];
                    results.Add(String.Format("'%query%': {0} | {1}",
                                              webResult.Description,
                                              webResult.Url));
                }
                else
                {
                    results.Add(String.Format("'%query%': No Results."));
                }
            }
            catch (Exception)
            {
                results.Add("The service is currently b00rked, please try again in a few minutes.");
            }

            // Call back our callback from our container.
            ((Action<string[]>)e.UserState).Invoke(results.ToArray());     
        }
    }
}
