/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using System.Web;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Linq;

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
    public class GoogleAjaxSearchProvider : AbstractReferrerSearchProvider
    {
        #region Constants

        private static readonly Uri SearchUri = new Uri("http://ajax.googleapis.com/ajax/services/search/");
        private const string ApiVersion = "v=1.0";

        #endregion

        #region Inner Classes

        private class State
        {
            internal string SearchCriteria { get; set; }
            internal Action<string[]> CallbackHandler { get; set; }
            internal WebClient WebClient { get; set; }
        }

        #endregion

        #region Methods

        public override void ExecuteSearch(string searchCriteria, string domain, Action<string[]> callbackHandler)
        {
            WebClient webClient = new WebClient();
            webClient.Headers.Add("Referer", Referrer.AbsoluteUri);
            webClient.DownloadStringCompleted += SearchCompletionHandler;

            //AJ: Currently only web search is supported
            Uri searchUri = BuildQueryUri(searchCriteria, "web");

            State state = new State
            {
                SearchCriteria = searchCriteria,
                CallbackHandler = callbackHandler,
                WebClient = webClient
            };

            webClient.DownloadStringAsync(searchUri, state);
        }

                /// <summary>
        /// Our callback to use when we've received our data from our "provider."
        /// </summary>
        /// 
        /// <param name="sender">Ignored.</param>
        /// <param name="e">The respose from our provider, including the state that we shoved in.</param>
        private void SearchCompletionHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            State state = (State)e.UserState;
            List<string> results = new List<string>();

            try
            {
                JObject json = JObject.Parse(e.Result);

                if (json["responseData"]["results"].Children().Count() > 0)
                {
                    var result = json["responseData"]["results"].Children().First();

                    // Grab the actual URL. We're going to need to strip off the quotes that Google hands back to us.
                    var resultUrl = result["url"].ToString().Replace("\"", "");

                    results.Add(String.Format("'{0}': {1} | {2}",
                                              state.SearchCriteria,
                                              result["titleNoFormatting"],
                                              resultUrl));
                }
                else
                {
                    results.Add(String.Format("'{0}': No Results.", state.SearchCriteria));
                }
            }
            catch (Exception)
            {
                results.Add("The service is currently b00rked, please try again in a few minutes.");
            }
            finally
            {
                state.WebClient.DownloadStringCompleted -= SearchCompletionHandler;
                state.WebClient.Dispose();
            }

            // Call back our callback from our container.
            state.CallbackHandler.Invoke(results.ToArray());
        }

        private static Uri BuildQueryUri(string searchCriteria, string searchType)
        {
            return new Uri(string.Format("{0}{1}?{2}&q={3}", SearchUri.AbsoluteUri, searchType, ApiVersion, HttpUtility.UrlEncode(searchCriteria)));
        }

        #endregion
    }
}