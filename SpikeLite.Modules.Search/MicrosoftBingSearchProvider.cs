/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2013 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;

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
        #region Inner Classes
        private class Metadata
        {
            public string uri { get; set; }
            public string type { get; set; }
        }

        private class Result
        {
            public Metadata __metadata { get; set; }
            public string ID { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string DisplayUrl { get; set; }
            public string Url { get; set; }
        }

        private class D
        {
            public List<Result> results { get; set; }
            public string __next { get; set; }
        }

        private class RootObject
        {
            public D d { get; set; }
        }
        #endregion

        public override void ExecuteSearch(string searchCriteria, string domain, Action<string[]> callbackHandler)
        {
            var domainSpecificSearchCriteria = searchCriteria.Trim();

            // Not everything needs a domain qualifier - BING and Google being the primary cases. Only add the domain if required, this will
            // prevent BING from searching MS docs no one cares about.
            if (!String.IsNullOrWhiteSpace(domain))
            {
                domainSpecificSearchCriteria += String.Format(" site:{0}", domain);
            }

            var searchUri = BuildQueryUri(domainSpecificSearchCriteria);

            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.Credentials = new NetworkCredential(ApiKey, ApiKey);
                httpClientHandler.PreAuthenticate = true;

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    httpClient.GetAsync(searchUri).ContinueWith((getTask) =>
                    {
                        var response = getTask.Result;

                        response.Content.ReadAsStringAsync().ContinueWith(readAsStringTask =>
                        {
                            var content = readAsStringTask.Result;
                            var customSearchResults = JsonConvert.DeserializeObject<RootObject>(content);
                            var results = new List<string>();

                            foreach (var item in customSearchResults.d.results.Take(1))
                            {
                                results.Add(String.Format("'%query%': {0} | {1}",
                                          item.Title,
                                          item.Url));
                            }

                            callbackHandler(results.ToArray());
                        });
                    }).Wait();
                }
            }
        }

        private static Uri BuildQueryUri(string searchCriteria)
        {
            return new Uri(new Uri("https://api.datamarket.azure.com/Bing/Search/Web"), string.Format("?Query='{0}'&$top=1&$format=json&Adult='off'&Market='en-US'", HttpUtility.UrlEncode(searchCriteria)));
        }
    }
}