/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.Communications;
using SpikeLite.Modules.Search.com.msn.search.soap;
using System.Text.RegularExpressions;
using SpikeLite.AccessControl;

namespace SpikeLite.Modules.Search
{
    [Module("MSDN", "Search MSDN", "~msdn <search terms>", AccessLevel.Public)]
    public class MSDNSearchModule : SiteSearchModuleBase
    {
        #region Constructors
        public MSDNSearchModule()
            : base("MSDN", "msdn", "msdn.microsoft.com")
        {
        }
        #endregion

        #region Methods

        #region PrepareResponse
        protected override Response PrepareResponse(string searchTerms, SearchResponse searchResponse, Request request)
        {
            Response response;

            Regex regEx = new Regex(@"(\(VS\.\d+\))");
            string url = regEx.Replace(searchResponse.Responses[0].Results[0].Url, string.Empty);

            if (searchResponse.Responses[0].Results.Length > 0)
                response = request.CreateResponse(ResponseType.Public, "{0}, {1} '{2}': {3} | {4}", request.Nick, base.Name, searchTerms, searchResponse.Responses[0].Results[0].Description, url);
            else
                response = request.CreateResponse(ResponseType.Public, "{0}, {1} '{2}': No Results", request.Nick, base.Name, searchTerms);

            return response;
        }
        #endregion

        #endregion
    }
}