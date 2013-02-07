/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2013 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace SpikeLite.Modules.Search
{
    /// <summary>
    /// Provides a Google AJAX Search based search provider.
    /// </summary>
    /// 
    /// <remarks>
    /// The Microsoft Live Search provider is preferred for two reasons: the first being that Google API keys are hard to come by, and the
    /// second being that Microsoft allows a higher number of searches per day. This provider is only used, by default, for the Google search
    /// itself.
    /// </remarks>
    public class GoogleAjaxSearchProvider : AbstractApiKeySearchProvider
    {
        #region Constants

        private static readonly Uri SearchUri = new Uri("https://www.googleapis.com/customsearch/v1");

        #endregion

        #region Inner Classes

        private class Url
        {
            public string type { get; set; }
            public string template { get; set; }
        }

        private class NextPage
        {
            public string title { get; set; }
            public int totalResults { get; set; }
            public string searchTerms { get; set; }
            public int count { get; set; }
            public int startIndex { get; set; }
            public string inputEncoding { get; set; }
            public string outputEncoding { get; set; }
            public string cx { get; set; }
        }

        private class Request
        {
            public string title { get; set; }
            public int totalResults { get; set; }
            public string searchTerms { get; set; }
            public int count { get; set; }
            public int startIndex { get; set; }
            public string inputEncoding { get; set; }
            public string outputEncoding { get; set; }
            public string cx { get; set; }
        }

        private class Queries
        {
            public List<NextPage> nextPage { get; set; }
            public List<Request> request { get; set; }
        }

        private class Context
        {
            public string title { get; set; }
        }

        private class Item
        {
            public string kind { get; set; }
            public string title { get; set; }
            public string htmlTitle { get; set; }
            public string link { get; set; }
            public string displayLink { get; set; }
            public string snippet { get; set; }
            public string htmlSnippet { get; set; }
        }

        private class GoogleCustomSearchResults
        {
            public string kind { get; set; }
            public Url url { get; set; }
            public Queries queries { get; set; }
            public Context context { get; set; }
            public List<Item> items { get; set; }
        }

        #endregion

        #region Methods

        public override void ExecuteSearch(string searchCriteria, ICollection<string> restrictToDomains, ICollection<string> excludeDomains, Action<string[]> callbackHandler)
        {
            if ((restrictToDomains != null && restrictToDomains.Count > 0) || (excludeDomains != null && excludeDomains.Count > 0))
                throw new NotImplementedException("Restrict to and exclude domains have not yet been implimented.");             

            var searchUri = BuildQueryUri(ApiKey, searchCriteria);

            using (var httpClient = new HttpClient())
            {
                httpClient.GetAsync(searchUri).ContinueWith((getTask) =>
                {
                    var response = getTask.Result;

                    response.Content.ReadAsStringAsync().ContinueWith(readAsStringTask =>
                    {
                        var content = readAsStringTask.Result;
                        var customSearchResults = JsonConvert.DeserializeObject<GoogleCustomSearchResults>(content);
                        var results = new List<string>();

                        foreach (var item in customSearchResults.items.Take(1))
                        {
                            results.Add(String.Format("'%query%': {0} | {1}",
                                      item.title,
                                      item.link));
                        }

                        callbackHandler(results.ToArray());
                    });
                }).Wait();
            }
        }

        private static Uri BuildQueryUri(string apiKey, string searchCriteria)
        {
            return new Uri(new Uri("https://www.googleapis.com/customsearch/v1"), string.Format("?key={0}&q={1}&cref=", apiKey, HttpUtility.UrlEncode(searchCriteria)));
        }

        #endregion
    }
}