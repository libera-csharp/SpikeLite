/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2013 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Collections.Generic;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Modules.Search
{
    /// <summary>
    /// Provides a prototype for basic searching. Given a search strategy and some basic information (URL, trigger etc),
    /// we attempt to execute a search. There are extensibility points to both filter the results before submission,
    /// and post find. If you need anything more advanced you can either subclass this class or write your own.
    /// </summary>
    public class SearchModulePrototype : ModuleBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the literal trigger the implementing class will respond to (ie: ~DADS).
        /// </summary>
        public string SearchTrigger { get; set; }

        /// <summary>
        /// Gets or sets the search provider backing this module.
        /// </summary>
        public ISearchProvider SearchProvider { get; set; }

        /// <summary>
        /// Gets or sets a list of domains to restrict the search to.
        /// </summary>
        public IList<string> RestrictToDomains { get; set; }

        /// <summary>
        /// Gets or sets a list of domains to exclude from the search.
        /// </summary>
        public IList<string> ExcludeDomains { get; set; }

        #endregion

        #region Searching

        /// <summary>
        /// Attempts to process our search request.
        /// </summary>
        /// 
        /// <param name="request">Our incoming message.</param>
        public override void HandleRequest(Request request)
        {
            if (request.RequestType == RequestType.Public)
            {
                var messageArray = request.Message.Split(' ');

                if (request.Message.StartsWith("~")
                    && request.RequestFrom.AccessLevel >= AccessLevel.Public
                    && messageArray.Length >= 2
                    && messageArray[0].ToLower() == SearchTrigger)
                {
                    var searchTerms = string.Empty;

                    for (var i = 1; i < messageArray.Length; i++)
                    {
                        searchTerms += messageArray[i] + " ";
                    }

                    Logger.DebugFormat("{0} - HandleRequest called on for {1} by {2}", Name, request.Message,
                                       request.Addressee ?? request.Nick);

                    SearchProvider.ExecuteSearch(searchTerms, RestrictToDomains, ExcludeDomains, x => HandleResults(x, request, searchTerms));
                }

                else if (messageArray[0].StartsWith(NetworkConnectionInformation.BotNickname))
                {
                    if (request.RequestFrom.AccessLevel >= AccessLevel.Public
                        && messageArray.Length >= 3
                        && messageArray[1].ToLower() == SearchTrigger.Substring(1))
                    {
                        var searchTerms = string.Empty;

                        for (var i = 2; i < messageArray.Length; i++)
                        {
                            searchTerms += messageArray[i] + " ";
                        }

                        Logger.DebugFormat("{0} - HandleRequest called on for {1} by {2}", Name, request.Message,
                                           request.Addressee ?? request.Nick);

                        SearchProvider.ExecuteSearch(searchTerms, RestrictToDomains, ExcludeDomains, x => HandleResults(x, request, searchTerms));
                    }
                }
            }
        }

        #endregion

        #region Message Filtering

        private void HandleResults(IEnumerable<string> results, Request request, string searchTerms)
        {
            // Not every search provider returns us our query strings, so do a quick token substitution for the services that don't. 
            foreach (var result in results)
            {
                var resultWithQuery = result.Replace("%query%", searchTerms.Trim());

                ModuleManagementContainer.HandleResponse(
                    request.CreateResponse(ResponseType.Public, "{0}, {1} {2}", request.Addressee ?? request.Nick, Name,
                                           resultWithQuery));
            }
        }

        #endregion
    }
}