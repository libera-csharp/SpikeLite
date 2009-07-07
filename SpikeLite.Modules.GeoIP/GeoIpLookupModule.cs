/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.Communications;
using SpikeLite.Modules.GeoIP.net.webservicex.www;
using System.Text.RegularExpressions;
using System;
using SpikeLite.Persistence.Authentication;

namespace SpikeLite.Modules.GeoIP
{
    /// <summary>
    /// A module to facilitate GeoIP lookups, based on quad-dotted notation.
    /// </summary>
    /// 
    /// <remarks>
    /// The module will not perform a name lookup. This is a basically a glorified async
    /// web call, so expect delay.
    /// </remarks>
    public class GeoIpLookupModule : ModuleBase
    {
        #region Data Members

        /// <summary>
        /// A regex grouped such that we only care about the IP.
        /// </summary>
        private const string GeoIpRegexPattern = @"~geoip\s(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";
        
        /// <summary>
        /// A regular expression object for match checking and grouping.
        /// </summary>
        private static readonly Regex GeoIpRegex = new Regex(GeoIpRegexPattern);

        /// <summary>
        /// Holds a reference to our WS proxy.
        /// </summary>
        private readonly GeoIPService _serviceProxy;

        #endregion

        #region Construction

        /// <summary>
        /// Creates our proxy, wires up our callback.
        /// </summary>
        public GeoIpLookupModule()
        {
            _serviceProxy = new GeoIPService();
            _serviceProxy.GetGeoIPCompleted += IpLookupCompletionHandler;
        }

        #endregion

        #region Behavior

        public override void HandleRequest(Request request)
        {
            if (request.Message.StartsWith("~geoip") && request.RequestFrom.AccessLevel >= AccessLevel.Public)
            {
                Match expressionMatch = GeoIpRegex.Match(request.Message);

                if (expressionMatch.Success)
                {
                    try
                    {
                        _serviceProxy.GetGeoIPAsync(expressionMatch.Groups[1].Value, request);
                    }
                    catch (Exception ex)
                    {
                       
                        ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Public, 
                                                                    "{0}, The service is currently b00rked, please try again in a few minutes.", request.Nick));
                        Logger.InfoFormat("Search threw an exception. Nick: {0}, terms: \"{1}\", stack message: {2}", request.Nick, expressionMatch.Groups[1].Value, ex.StackTrace);
                    }
                    
                }
                else
                {
                    // If the message format isn't correct, notify the user.
                    ModuleManagementContainer.HandleResponse(request.CreateResponse(
                                                 GetResponseTypeForRequestType(request.RequestType),
                                                 String.Format("{0}, invalid geoip syntax, please try again.", request.Nick)));
                }
            }
        }

        /// <summary>
        /// Our async callback, called when our GeoIP lookup is completed. This is a potentially expensive query.
        /// </summary>
        /// 
        /// <param name="sender">Our service proxy, ignored.</param>
        /// 
        /// <param name="e">Our GeoIP lookup containing our <see cref="GeoIP"/> response from the service.</param>
        private void IpLookupCompletionHandler(object sender, GetGeoIPCompletedEventArgs e)
        {
            Request requestContext = (Request)e.UserState;

            string response = string.Format("{0}, the IP {1} maps to the country '{2}', ISO code {3}.",
                                            requestContext.Addressee,
                                            e.Result.IP,
                                            e.Result.CountryName,
                                            e.Result.CountryCode);

            ModuleManagementContainer.HandleResponse(requestContext.CreateResponse(
                                         GetResponseTypeForRequestType(requestContext.RequestType), 
                                         response));
        }

        /// <summary>
        /// Do a quick translation between <see cref="RequestType"/> and <see cref="ResponseType"/>.
        /// </summary>
        /// 
        /// <param name="requestType">The <see cref="RequestType"/> you'd like a corresponding <see cref="ResponseType"/> for.</param>
        /// 
        /// <returns>The corresponding <see cref="ResponseType"/> for your request.</returns>
        private static ResponseType GetResponseTypeForRequestType(RequestType requestType)
        {
            return requestType.Equals(RequestType.Public) ? ResponseType.Public : ResponseType.Private;
        }

        #endregion 
    }
}
