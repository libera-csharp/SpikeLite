/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.Communications;
using SpikeLite.Modules.Search.com.msn.search.soap;
using SpikeLite.Persistence.Authentication;

namespace SpikeLite.Modules.Search
{
    [Module("Live", "Use Live.com to search.", "Usage Syntax: ~live <search terms>", AccessLevel.Public)]
    public class LiveSearchModule : ModuleBase
    {
        #region Fields
        private readonly MSNSearchService msnSearchService;
        #endregion

        #region Constructors
        public LiveSearchModule()
        {
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

                if (request.Message.StartsWith("~"))
                {
                    if (request.RequestFrom.AccessLevel >= AccessLevel.Public)
                    {
                        if (messageArray.Length >= 2)
                        {
                            if (messageArray[0].ToLower() == "~live")
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
                            if (messageArray[1].ToLower() == "live")
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

        private void ExecuteSearch( string searchTerms, Request request )
        {
            Response response;

            try
            {
                SearchRequest searchRequest = new SearchRequest
                {
                      AppID = configuration.Licenses["googleSoapApi"].Key,
                      CultureInfo = "en-GB",
                      Query = searchTerms.Trim(),
                      Requests = new SourceRequest[1]
                };

                searchRequest.Requests[0] = new SourceRequest
                {
                    Source = SourceType.Web,
                    ResultFields = (ResultFieldMask.Url | ResultFieldMask.Description)
                };

                SearchResponse searchResponse = msnSearchService.Search(searchRequest);
                
                response = searchResponse.Responses.Length > 0 ? request.CreateResponse(ResponseType.Public, "{0}, {1}: {2} | {3}", request.Nick, searchTerms, searchResponse.Responses[0].Results[0].Description, searchResponse.Responses[0].Results[0].Url) : request.CreateResponse(ResponseType.Public, "{0}, {1}: No Results", request.Nick, searchTerms);
            }
            catch
            {
                response = request.CreateResponse(ResponseType.Public, "{0}, The service is currently b00rked, please try again in a few minutes.", request.Nick);
            }

            ModuleManagementContainer.HandleResponse(response);
        }

        #endregion
    }
}