/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.Communications;
using SpikeLite.Modules.Search.com.google.api;
using SpikeLite.Modules.Search.Google;
using SpikeLite.Persistence.Authentication;

namespace SpikeLite.Modules.Search
{
    /// <summary>
    /// An async wrapper around the Google webservice API.
    /// </summary>
    [Module("Google", "Use google.com to search.", "Usage Syntax: ~google <search terms>", AccessLevel.Public)]
    public class GoogleSearchModule : ModuleBase
    {
        #region Data Members

        /// <summary>
        /// Handles our actual searching as well as callback registration.
        /// </summary>
        private readonly GoogleSearch _searchBroker;

        #endregion

        #region Constructors

        /// <summary>
        /// Spin up our proxy, wire up our callback.
        /// </summary>
        public GoogleSearchModule()
        {
            _searchBroker = new GoogleSearch();
            _searchBroker.SearchCompleted += SearchCompletedHandler;
        }
        #endregion

        #region Methods

        #region ModuleBase Behavior 

        /// <summary>
        /// Handle incoming requests to search.
        /// </summary>
        /// 
        /// <param name="request">A request context object. See <see cref="Request"/>.</param>
        protected override void InternalHandleRequest(Request request)
        {
            // TODO: Kog JUN-07 2008 - refactor this to use the new syntax.
            if (request.RequestType == RequestType.Public)
            {
                string[] messageArray = request.Message.Split(' ');

                if (request.Message.StartsWith("~"))
                {
                    if (request.RequestFrom.AccessLevel >= AccessLevel.Public)
                    {
                        if (messageArray.Length >= 2)
                        {
                            if (messageArray[0].ToLower() == "~google")
                            {
                                string searchTerms = string.Empty;

                                for (int i = 1; i < messageArray.Length; i++)
                                {
                                    searchTerms += messageArray[i] + " ";
                                }

                                ExecuteSearch(searchTerms, request);
                            }
                        }
                    }
                }
                else if (messageArray[0].StartsWith(Configuration.Networks["FreeNode"].BotNickname))
                {
                    if (request.RequestFrom.AccessLevel >= AccessLevel.Public)
                    {
                        if (messageArray.Length >= 3)
                        {
                            if (messageArray[1].ToLower() == "google")
                            {
                                string searchTerms = string.Empty;

                                for (int i = 2; i < messageArray.Length; i++)
                                {
                                    searchTerms += messageArray[i] + " ";
                                }

                                ExecuteSearch(searchTerms, request);
                            }
                        }
                    }
                }
            }
        }

        #endregion 

        #region Search Handling

        /// <summary>
        /// Attempt to run our async search.
        /// </summary>
        /// 
        /// <param name="searchTerms">A boolean search string.</param>
        /// <param name="request">A request context.</param>
        private void ExecuteSearch(string searchTerms, Request request)
        {
            try
            {
                searchTerms = searchTerms.Trim();
                _searchBroker.Search(searchTerms, request);
            }
            catch
            {
                Response response = request.CreateResponse(ResponseType.Public, "{0}, The service is currently b00rked, please try again in a few minutes.", request.Nick);
                ModuleManagementContainer.HandleResponse(response);
            }
        }

        /// <summary>
        /// A callback to return results to the searcher.
        /// </summary>
        /// 
        /// <param name="sender">The object calling the method. Ignored.</param>
        /// <param name="e">An event handed back to us by our WS proxy.</param>
        private void SearchCompletedHandler(object sender, doGoogleSearchCompletedEventArgs e)
        {
            Request request = (Request) e.UserState;
            Response response;

            try
            {
                if (e.Result.resultElements.Length > 0)
                {
                    response = request.CreateResponse(ResponseType.Public, "{0}, Google '{1}': {2} | {3}", request.Nick, e.Result.searchQuery, e.Result.resultElements[0].title.Replace("<b>", "").Replace(@"</b>", ""), e.Result.resultElements[0].URL);
                }
                else
                {
                    response = request.CreateResponse(ResponseType.Public, "{0}, Google '{1}': No Results.", request.Nick, e.Result.searchQuery);
                }
            }
            catch
            {
                response = request.CreateResponse(ResponseType.Public, "{0}, The service is currently b00rked, please try again in a few minutes.", request.Nick);
            }

            ModuleManagementContainer.HandleResponse(response);
        }

        #endregion

        #endregion
    }
}
