/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using SpikeLite.Modules.Search.com.google.api;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

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
        private static readonly string ApiVersion = "v=1.0";
        #endregion

        #region Properties
        public Uri ReferrerUri { get { return new Uri(Referrer); } }
        #endregion

        #region Inner Classes
        [Serializable]
        internal class WebSearchResponseMessage
        {
            #region Inner Classes
            [Serializable]
            internal class WebSearchResponseData
            {
                #region Inner Classes
                [Serializable]
                internal class WebSearchResultItem
                {
                    #region Fields
                    [OptionalField]
                    private string gsearchResultClass = null;

                    private string unescapedUrl = null;
                    private string url = null;
                    private string visibleUrl = null;
                    private string cacheUrl = null;
                    private string title = null;
                    private string titleNoFormatting = null;
                    private string content = null;
                    #endregion

                    #region Properties
                    internal string GsearchResultClass { get { return gsearchResultClass; } }
                    internal string UnescapedUrl { get { return unescapedUrl; } }
                    internal string Url { get { return url; } }
                    internal string VisibleUrl { get { return visibleUrl; } }
                    internal string CacheUrl { get { return cacheUrl; } }
                    internal string Title { get { return title; } }
                    internal string TitleNoFormatting { get { return titleNoFormatting; } }
                    internal string Content { get { return content; } }
                    #endregion
                }
                #endregion

                #region Fields
                private WebSearchResultItem[] results = null;
                private Cursor cursor = null;
                #endregion

                #region Properties
                internal WebSearchResultItem[] Results { get { return results; } }
                internal Cursor Cursor { get { return cursor; } }
                #endregion
            }

            //AJ: Moved here to prevent name clashes
            [Serializable]
            internal class Cursor
            {
                #region Inner Classes
                [Serializable]
                internal class Page
                {
                    #region Fields
                    private string start = string.Empty;
                    private string label = string.Empty;
                    #endregion

                    #region Properties
                    internal string Start { get { return start; } }
                    internal string Label { get { return label; } }
                    #endregion
                }
                #endregion

                #region Fields
                [OptionalField]
                private Page[] pages = null;

                [OptionalField]
                private string estimatedResultCount = null;

                [OptionalField]
                private string currentPageIndex = null;

                [OptionalField]
                private string moreResultsUrl = null;
                #endregion

                #region Properties
                internal Page[] Pages { get { return pages; } }
                internal string EstimatedResultCount { get { return estimatedResultCount; } }
                internal string CurrentPageIndex { get { return currentPageIndex; } }
                internal string MoreResultsUrl { get { return moreResultsUrl; } }
                #endregion
            }
            #endregion

            #region Fields
            [OptionalField]
            internal string rawText = string.Empty;

            internal string responseDetails = null;
            internal string responseStatus = null;
            private WebSearchResponseData responseData = new WebSearchResponseData();
            #endregion

            #region Properties
            internal string RawText { get { return rawText; } }
            internal string ResponseDetails { get { return responseDetails; } }
            internal string ResponseStatus { get { return responseStatus; } }
            internal WebSearchResponseData ResponseData { get { return responseData; } }
            #endregion
        }
        internal class State
        {
            public string SearchCriteria { get; set; }
            public Action<string[]> CallbackHandler { get; set; }
            public HttpWebRequest HttpWebRequest { get; set; }
        }
        #endregion

        #region Methods
        public override void ExecuteSearch(string searchCriteria, string domain, Action<string[]> callbackHandler)
        {
            //AJ: Currently only web search is supported
            Uri searchUri = BuildQueryUri(searchCriteria, "web");

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(searchUri);
            httpWebRequest.Referer = ReferrerUri.AbsoluteUri;

            State state = new State()
            {
                SearchCriteria = searchCriteria,
                CallbackHandler = callbackHandler,
                HttpWebRequest = httpWebRequest
            };

            IAsyncResult result = (IAsyncResult)httpWebRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), state);
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            State state = (State)asynchronousResult.AsyncState;
            HttpWebResponse httpWebResponse = (HttpWebResponse)state.HttpWebRequest.GetResponse();
            string response;

            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }

            WebSearchResponseMessage webSearchResponseMessage;

            using (MemoryStream memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(response)))
            {
                DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(WebSearchResponseMessage));
                webSearchResponseMessage = (WebSearchResponseMessage)dataContractJsonSerializer.ReadObject(memoryStream);
            }

            List<string> results = new List<string>();

            try
            {
                if (webSearchResponseMessage.ResponseData.Results.Length > 0)
                {
                    results.Add(String.Format("'{0}': {1} | {2}",
                                              state.SearchCriteria,
                                              webSearchResponseMessage.ResponseData.Results[0].TitleNoFormatting,
                                              webSearchResponseMessage.ResponseData.Results[0].Url));
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