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
using SpikeLite.Persistence.Authentication;

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
            Regex regEx = new Regex(@"(\(VS\.\d+\))");
            string url = regEx.Replace(searchResponse.Responses[0].Results[0].Url, string.Empty);

            return searchResponse.Responses[0].Results.Length > 0 ? request.CreateResponse(ResponseType.Public, "{0}, {1} '{2}': {3} | {4}", request.Addressee, Name, searchTerms, searchResponse.Responses[0].Results[0].Description, url) : request.CreateResponse(ResponseType.Public, "{0}, {1} '{2}': No Results", request.Nick, Name, searchTerms);
        }
        #endregion

        #endregion
    }
}