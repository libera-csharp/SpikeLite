/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Text;
using SpikeLite.Communications;
using SpikeLite.Modules.Search.com.msn.search.soap;
using SpikeLite.Persistence.Authentication;

namespace SpikeLite.Modules.Search
{
    /// <summary>
    /// This class forms the a parent class from which a given site-search module may inherit.
    /// </summary>
    public abstract class SiteSearchModuleBase : ModuleBase
    {
        /// <summary>
        /// Holds the name of our search provider, in a pretty-printed format.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Holds the trigger we respond to.
        /// </summary>
        private readonly string _command;

        /// <summary>
        /// Holds the actual URL we're searching against.
        /// </summary>
        private readonly string _site;

        /// <summary>
        /// Holds our default searching proxy.
        /// </summary>
        private readonly MSNSearchService _searchProxy;

        /// <summary>
        /// Gets the name of our search module.
        /// </summary>
        protected string Name {  get { return _name; } }

        public SiteSearchModuleBase(string name, string command, string site)
        {
            _name = name;
            _command = command;
            _site = site;

            _searchProxy = new MSNSearchService();
        }

        /// <summary>
        /// Parses incoming messages and tries to create search requests.
        /// </summary>
        /// 
        /// <param name="request">Our incoming message.</param>
        protected override void InternalHandleRequest(Request request)
        {
            if (request.RequestType == RequestType.Public)
            {
                string[] messageArray = request.Message.Split(' ');

                if (request.Message.StartsWith("~")
                    && request.RequestFrom.AccessLevel >= AccessLevel.Public
                    && messageArray.Length >= 2
                    && messageArray[0].ToLower() == "~" + _command)
                {
                    StringBuilder searchTerms = new StringBuilder();

                    for (int i = 1; i < messageArray.Length; i++)
                    {
                        searchTerms.Append(messageArray[i]);
                        searchTerms.Append(" ");
                    }

                    ExecuteSearch(searchTerms.ToString(), request);

                    searchTerms.Length = 0;
                }
                else if (messageArray[0].ToLower().StartsWith(Configuration.Networks["FreeNode"].BotNickname.ToLower())
                    && request.RequestFrom.AccessLevel >= AccessLevel.Public
                    && messageArray.Length >= 3
                    && messageArray[1].ToLower() == _command)
                {
                    StringBuilder searchTerms = new StringBuilder();

                    for (int i = 2; i < messageArray.Length; i++)
                    {
                        searchTerms.Append(messageArray[i]);
                        searchTerms.Append(" ");
                    }

                    ExecuteSearch(searchTerms.ToString(), request);

                    searchTerms.Length = 0;
                }
            }
        }

        /// <summary>
        /// A convenience method, used to sanitize our incoming search terms.
        /// </summary>
        /// 
        /// <param name="searchTerms">A string of search terms to sanitize.</param>
        /// 
        /// <returns>A sanizited set of search terms.</returns>
        protected virtual string PrepareSearchTerms(string searchTerms)
        {
            return searchTerms.Trim();
        }

        /// <summary>
        /// Attempts to pretty print a response to be sent back to a user.
        /// </summary>
        /// 
        /// <param name="searchTerms">The set of search terms used.</param>
        /// <param name="searchResponse">Our search hit, if any.</param>
        /// <param name="request">Our initial message asking us to search, providing context as to who sent it.</param>
        /// 
        /// <returns>A response fit to be sent back to our communications manager.</returns>
        protected virtual Response PrepareResponse(string searchTerms, SearchResponse searchResponse, Request request)
        {
            return searchResponse.Responses[0].Results.Length > 0 ? request.CreateResponse(ResponseType.Public, "{0}, {1} '{2}': {3} | {4}", 
                                                                                           request.Addressee, 
                                                                                           _name, 
                                                                                           searchTerms, 
                                                                                           searchResponse.Responses[0].Results[0].Description, 
                                                                                           searchResponse.Responses[0].Results[0].Url) 
                                                                   : request.CreateResponse(ResponseType.Public, "{0}, {1} '{2}': No Results", 
                                                                                            request.Addressee, 
                                                                                            _name, 
                                                                                            searchTerms);
        }

        /// <summary>
        /// Attempts to execute an async webservices based search using our proxy assigned above. 
        /// </summary>
        /// 
        /// <param name="searchTerms">A (hopefully) sanitized set of terms to search on.</param>
        /// <param name="request">Our request context.</param>
        private void ExecuteSearch(string searchTerms, Request request)
        {
            Response response;

            try
            {
                SearchRequest searchRequest = new SearchRequest
                                              {
                                                  AppID = ApiKey,
                                                  CultureInfo = "en-GB",
                                                  Query =
                                                      (PrepareSearchTerms(searchTerms) +
                                                       string.Format(" site:{0}", _site)),
                                                  Requests = new SourceRequest[1]
                                              };

                searchRequest.Requests[0] = new SourceRequest
                                            {
                                                Source = SourceType.Web,
                                                ResultFields = (ResultFieldMask.Url | ResultFieldMask.Description)
                                            };

                SearchResponse searchResponse = _searchProxy.Search(searchRequest);

                response = PrepareResponse(searchTerms, searchResponse, request);
            }
            catch (Exception ex)
            {
                response = request.CreateResponse(ResponseType.Public, "{0}, The service is currently b00rked, please try again in a few minutes.", request.Nick);
                Logger.InfoFormat("Search threw an exception. Nick: {0}, terms: \"{1}\", stack message: {2}", request.Nick, searchTerms, ex.StackTrace);
            }

            ModuleManagementContainer.HandleResponse(response);
        }
    }
}