/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Net;
using Mono.WebServer;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;
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

        // TODO: Kog 12/5/2009 - It doesn't look like there's any way to query XSP to see what it's doing... so for now I just hacked out a boolean. This requires
        // TODO: Kog 12/5/2009 - more work.

        /// <summary>
        /// Stores whether or not the appserver has been spun up. Primarily useful for doing start/stop operations.
        /// </summary>
        private bool _isAppServerStarted;

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

        /// <summary>
        /// Constructs our module, configures XSP2 and launches it.
        /// </summary>
        public XspHost()
        {
            // TODO: Kog 12/5/2009 - Add configuration for IPs... I think we can leave this at ANY for now though.
            _appServer = new ApplicationServer(new XSPWebSource(IPAddress.Any, Port)) { Verbose = true };
            _appServer.AddApplicationsFromCommandLine(string.Format("{0}:/:{1}", Port, Path));

            // TODO: Kog 12/5/2009 - Do we want this to be done by parsing messages below, or to auto start?
            StartAppServer();
        }

        public override void HandleRequest(Request request)
        {
            if (request.RequestFrom.AccessLevel >= AccessLevel.Root && request.Message.StartsWith("~xsp", StringComparison.OrdinalIgnoreCase))
            {
                string[] splitMessage = request.Message.Split(' ');
                string operationDescription = splitMessage[1];

                if (operationDescription.Equals("start", StringComparison.OrdinalIgnoreCase))
                {
                    // Make sure we're not started already.
                    if (!_isAppServerStarted)
                    {
                        StartAppServer();

                        Logger.InfoFormat("{0} started the XSP server.", request.Nick);
                        ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Public, "XSP server started."));
                    }
                    else
                    {
                        ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Public, "The XSP server is already running, cannot start it."));
                    }
                }

                if (operationDescription.Equals("stop", StringComparison.OrdinalIgnoreCase))
                {
                    // Make sure we're started already. 
                    if (_isAppServerStarted)
                    {
                        StopAppServer();

                        Logger.InfoFormat("{0} stopped the XSP server.", request.Nick);
                        ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Public, "XSP server stopping..."));
                    }
                    else
                    {
                        ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Public, "The XSP server must already be running to stop it."));    
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to start the XSP app server.
        /// </summary>
        private void StartAppServer()
        {
            _appServer.Start(true);
            _isAppServerStarted = true;
        }

        /// <summary>
        /// Attempts to stop the XSP app server.
        /// </summary>
        /// 
        /// <remarks>
        /// It's worth noting that there's a significant time laps between calling Stop() and the actual appserver stopping.
        /// </remarks>
        private void StopAppServer()
        {
            _appServer.Stop();
            _isAppServerStarted = false;
        }
    }
}
