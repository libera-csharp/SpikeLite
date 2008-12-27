/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.Modules.Search.com.google.api;
using SpikeLite.Communications;

namespace SpikeLite.Modules.Search.Google
{
    /// <summary>
    /// Attempts to do an async lookup via Google's webservice API.
    /// </summary>
    public class GoogleSearch
    {
        /// <summary>
        /// Holds the reference to the actual WS proxy.
        /// </summary>
        private readonly GoogleSearchService _googleSearchService;

        /// <summary>
        /// Allows registration of callbacks to be called upon search completion.
        /// </summary>
        public event doGoogleSearchCompletedEventHandler SearchCompleted
        {
            add { _googleSearchService.doGoogleSearchCompleted += value; }
            remove { _googleSearchService.doGoogleSearchCompleted -= value; }
        }

        /// <summary>
        /// Gets or sets our Google API key.
        /// </summary>
        /// 
        /// <remarks>
        /// We need to redesign the search hierarchy, this isn't a very good strategy.
        /// </remarks>
        public string ApiKey
        {
            get; set;
        }

        #region Constructors

        /// <summary>
        /// Wire up our proxy object.
        /// </summary>
        public GoogleSearch()
        {
            _googleSearchService = new GoogleSearchService();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Attempts to search for a given set of search terms.
        /// </summary>
        /// 
        /// <param name="searchTerms">A string of boolean search terms.</param>
        /// <param name="userState">A response context to pass back to our callback.</param>
        /// 
        /// <remarks>
        /// Requires the key "GoogleAPIKey" to be set to a valid Google API key in your app.config.
        /// </remarks>
        public void Search(string searchTerms, Request userState)
        {
            _googleSearchService.doGoogleSearchAsync
            (
                ApiKey,
                searchTerms,
                0,
                1,
                false,
                string.Empty,
                true,
                "lang_en",
                string.Empty,
                string.Empty,
                userState
            );
        }

        #endregion
    }
}