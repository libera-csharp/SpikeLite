/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Net;
using Mono.WebServer;
using SpikeLite.Communications;
using SpikeLite.Modules;

// TODO: Kog 12/5/2009 - Maybe we can wire this up w/ Spring.NET somehow?

namespace SpikeLite.IPC.WebHost
{
    /// <summary>
    /// This class allows us to bolt on XSP as an IPC host for HTTP. This allows us to do things like host ASP.NET or Webservice endpoints.
    /// </summary>
    public class XspHost : ModuleBase
    {

        #region Data Members and Properties 

        /// <summary>
        /// Holds the default port value (8080).
        /// </summary>
        private int _port = 8080;

        /// <summary>
        /// Holds the default XSP binding path (.\Web).
        /// </summary>
        private string _path = "Web";

        /// <summary>
        /// Holds a reference to the application server we're about to bind.
        /// </summary>
        private readonly ApplicationServer _appServer;

        /// <summary>
        /// Gets or sets the port that we should bind to. This defaults to 8080.
        /// </summary>
        public int Port
        {
            get { return _port; } 
            set { _port = value; }
        }

        /// <summary>
        /// Gets or sets the application context for hosting. This defaults to "Web."
        /// </summary>
        /// 
        /// <remarks>
        /// Due to the way that XSP hosts things, make sure that if you move from web to another directory that you have both the stuff to be hosted (ASMX files, HTML etc)
        /// as well as the bin\ directory with any assemblies that may need to be bound (say, WS endpoint logic).
        /// </remarks>
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        #endregion

        public XspHost()
        {
            // TODO: Kog 12/5/2009 - Add configuration for IPs... I think we can leave this at ANY for now though.
            _appServer = new ApplicationServer(new XSPWebSource(IPAddress.Any, Port)) { Verbose = true };
            _appServer.AddApplicationsFromCommandLine(string.Format("{0}:/:{1}", Port, Path));

            // TODO: Kog 12/5/2009 - Do we want this to be done by parsing messages below, or to auto start?
            _appServer.Start(true);
        }

        public override void HandleRequest(Request request)
        {
            // TODO: Kog 12/5/2009 - Consume a stop message, such that we can shut down XSP without killing the bot.
        }
    }
}
