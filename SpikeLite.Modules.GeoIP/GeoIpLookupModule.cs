/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;
using SpikeLite.Communications;
using System.Text.RegularExpressions;
using System;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Modules.GeoIP
{
    /// <summary>
    /// A module to facilitate GeoIP lookups, based on quad-dotted notation.
    /// </summary>
    /// 
    /// <remarks>
    /// This module uses the HostIP.info plain XML "api," where we make an HTTP/GET request and get back an XML document describing the information we want.
    /// We then parse the XML using XLINQ and spit out either the information the user wants, or a lack of informaiton should the IP not be in the DB.
    /// 
    /// No guarantees are made about the integrity of the provider or of the data that is returned. This is also a public service with no SLAs, so you gets
    /// what you gets.
    /// </remarks>
    public class GeoIpLookupModule : ModuleBase
    {
        #region Data Members

        // TODO: Kog 07/08/2009 - Can we add an NSlookup call for non quad dots? How bad of an idea would that be?

        /// <summary>
        /// A regex grouped such that we only care about the IP.
        /// </summary>
        private const string GeoIpRegexPattern = @"~geoip\s(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";

        /// <summary>
        /// A regular expression object for match checking and grouping.
        /// </summary>
        private static readonly Regex GeoIpRegex = new Regex(GeoIpRegexPattern);

        /// <summary>
        /// Holds a template const that we can use for the HostIP.info URL.
        /// </summary>
        private static readonly Uri SearchUri = new Uri("http://api.hostip.info/");

        /// <summary>
        /// Provides a response template that we can use to respond to users. 
        /// </summary>
        private const string ResponseTemplate = "{0}, the IP {1} maps to the country '{2}', ISO code {3}";

        #endregion

        #region Behavior

        public override void HandleRequest(Request request)
        {
            if (request.Message.StartsWith("~geoip") && request.RequestFrom.AccessLevel >= AccessLevel.Public)
            {
                var expressionMatch = GeoIpRegex.Match(request.Message);

                if (expressionMatch.Success)
                {
                    try
                    {
                        ExecuteSearch(expressionMatch.Groups[1].Value, request);
                    }
                    catch (Exception ex)
                    {

                        ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Public,
                                                                 "{0}, The service is currently b00rked, please try again in a few minutes.", request.Nick));
                        Logger.WarnFormat("Search threw an exception. Nick: {0}, terms: \"{1}\", stack message: {2}",
                                          request.Nick, expressionMatch.Groups[1].Value, ex.StackTrace);
                    }

                }
                else
                {
                    // If the message format isn't correct, notify the user.
                    ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Public,
                                                                                    String.Format("{0}, invalid geoip syntax, please try again.",
                                                                                    request.Nick)));
                }
            }
        }

        /// <summary>
        /// Performs a search for information on the given criteria (in this case a quad-dotted IP).
        /// </summary>
        /// 
        /// <param name="ipAddress">The IPv4 IP, in quad-dotted notation, to search for.</param>
        /// <param name="request">Context about our request, useful for constructing our ResponseTemplate to the user.</param>
        private void ExecuteSearch(string ipAddress, Request request)
        {
            var searchUri = new Uri(string.Format("{0}?ip={1}", SearchUri.AbsoluteUri, HttpUtility.UrlEncode(ipAddress)));

            var webClient = new WebClient();
            webClient.DownloadStringCompleted += SearchCompletionHandler;
            webClient.DownloadStringAsync(searchUri, new Tuple<Request, string, WebClient>(request, ipAddress, webClient));
        }

        /// <summary>
        /// Our callback to use when we've received our data from our "provider."
        /// </summary>
        /// 
        /// <param name="sender">Ignored.</param>
        /// <param name="e">The respose from our provider, including the state that we shoved in.</param>
        private void SearchCompletionHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            // Blargh this is nasty. Pull out our context and start casting crap.
            var userContext = (Tuple<Request, string, WebClient>)e.UserState;
            var requestContext = userContext.Item1;
            var ip = userContext.Item2;
            var webclient = userContext.Item3;

            string response;

            try
            {
                // Construct an XLinq document fragment and start anonymously pulling things out.
                var xmlResponse = XDocument.Parse(e.Result);


                var root = xmlResponse.Descendants("Hostip");
                response = String.Format(ResponseTemplate,
                                         requestContext.Addressee ?? requestContext.Nick,
                                         ip,
                                         root.Elements("countryName").FirstOrDefault().Value,
                                         root.Elements("countryAbbrev").FirstOrDefault().Value);
            }
            catch (NullReferenceException ex)
            {
                Logger.WarnFormat("GeoIPLookupModule search failure for IP {0} by {1}. Message: {2}", ip, requestContext.Nick, ex.Message);
                response = "No such IP, or the service lookup has failed.";
            }
            finally
            {
                webclient.DownloadStringCompleted -= SearchCompletionHandler;
                webclient.Dispose();
            }

            ModuleManagementContainer.HandleResponse(requestContext.CreateResponse(ResponseType.Public, response));
        }

        #endregion
    }
}