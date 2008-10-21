/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System.Text;
using SpikeLite.Communications;
using SpikeLite.Modules.Search.com.msn.search.soap;
using SpikeLite.Persistence.Authentication;

namespace SpikeLite.Modules.Search
{
    public abstract class SiteSearchModuleBase : ModuleBase
    {
        #region Fields
        private readonly string name;
        private readonly string command;
        private readonly string site;

        private readonly MSNSearchService msnSearchService;
        #endregion

        #region Properties

        #region Name
        protected string Name {  get { return name; } }
        #endregion

        #endregion

        #region Constructors

        public SiteSearchModuleBase(string name, string command, string site)
        {
            this.name = name;
            this.command = command;
            this.site = site;

            msnSearchService = new MSNSearchService();
        }

        #endregion

        #region Methods

        #region InternalHandleRequest

        protected override void InternalHandleRequest(Request request)
        {
            if (request.RequestType == RequestType.Public)
            {
                string[] messageArray = request.Message.Split(' ');

                if (request.Message.StartsWith("~")
                    && request.RequestFrom.AccessLevel >= AccessLevel.Public
                    && messageArray.Length >= 2
                    && messageArray[0].ToLower() == "~" + command)
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
                    && messageArray[1].ToLower() == command)
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

        #endregion

        #region PrepareSearchTerms
        protected virtual string PrepareSearchTerms(string searchTerms)
        {
            return searchTerms.Trim();
        }
        #endregion

        #region PrepareResponse
        protected virtual Response PrepareResponse(string searchTerms, SearchResponse searchResponse, Request request)
        {
            return searchResponse.Responses[0].Results.Length > 0 ? request.CreateResponse(ResponseType.Public, "{0}, {1} '{2}': {3} | {4}", request.Nick, name, searchTerms, searchResponse.Responses[0].Results[0].Description, searchResponse.Responses[0].Results[0].Url) : request.CreateResponse(ResponseType.Public, "{0}, {1} '{2}': No Results", request.Nick, name, searchTerms);
        }
        #endregion

        #region ExecuteSearch

        private void ExecuteSearch(string searchTerms, Request request)
        {
            Response response;

            try
            {
                SearchRequest searchRequest = new SearchRequest
                                              {
                                                  AppID = Configuration.Licenses["liveAppID"].Key,
                                                  CultureInfo = "en-GB",
                                                  Query =
                                                      (PrepareSearchTerms(searchTerms) +
                                                       string.Format(" site:{0}", site)),
                                                  Requests = new SourceRequest[1]
                                              };

                searchRequest.Requests[0] = new SourceRequest
                                            {
                                                Source = SourceType.Web,
                                                ResultFields = (ResultFieldMask.Url | ResultFieldMask.Description)
                                            };

                SearchResponse searchResponse = msnSearchService.Search(searchRequest);

                response = PrepareResponse(searchTerms, searchResponse, request);
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