/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */


using SpikeLite.Domain.Model.Authentication;
using SpikeLite.Modules;
using log4net.Ext.Trace;
using SpikeLite.Communications;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace SpikeLite.IPC.WebHost.Services
{
    /// <summary>
    /// This class does all the WCF setup for our Ping webservice endpoint.
    /// </summary>
    public class PingServiceHostModule : ModuleBase
    {
        #region Data Members

        /// <summary>
        /// Stores our log4net logger.
        /// </summary>
        private readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(PingServiceHostModule));

        /// <summary>
        /// Holds a reference to the <see cref="ServiceHost"/> we're going to use to host our WS endpoints.
        /// </summary>
        private ServiceHost _serviceHost;

        /// <summary>
        /// Holds a flag, representing whether or not our WCF <see cref="ServiceHost"/> has been started. This allows us to do things like
        /// starting and stopping service.
        /// </summary>
        private bool _serviceStarted;

        /// <summary>
        /// Holds the binding address for this service.
        /// </summary>
        private readonly Uri _baseAddress = new Uri("http://localhost:8081/SpikeLite/Ping");

        #endregion

        /// <summary>
        /// Register our WCF endpoint.
        /// </summary>
        /// 
        ///
        /// <remarks>
        /// Users on Vista or greater might have to reserve the endpoint via Netsh.exe as follows:
        /// 
        /// <code>
        /// netsh http add urlacl url=http://+:8081/SpikeLite/Ping user=DOMAIN\user 
        /// </code>  
        /// 
        /// Please make sure to run your cmd session with administrative rights.
        /// </remarks>
        public PingServiceHostModule()
        {
            try
            {
                CreateServiceHost();

                _logger.InfoFormat("Opening servicehost for PingServiceHost via URI {0}", _baseAddress.AbsolutePath);

                _serviceHost.Open();
                _serviceStarted = true;
            }
            catch (CommunicationException ce)
            {
                _logger.WarnFormat("Caught an exception trying to bind our WCF endpoint: {0}", ce);
            }
        }

        public override void HandleRequest(Request request)
        {
            string[] message = request.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (request.RequestFrom.AccessLevel >= AccessLevel.Root && 
                message[0].Equals("~pinghost", StringComparison.OrdinalIgnoreCase) && 
                message.Length == 2)
            {
                string resultMessage = "Unknown pinghost operation. Please specify one of the two operations: start, stop, status.";

                // Someone is attempting to start our servicehost...
                if (message[1].Equals("start", StringComparison.OrdinalIgnoreCase))
                {
                    // Naughty zoot, don't double-start.
                    if (_serviceStarted)
                    {
                        _logger.WarnFormat("{0} is attempting to start the PingService servicehost, but it is already started.", request.Nick);
                        resultMessage = "Failed attempting to start the PingService servicehost, it's already started.";
                    }
                    else
                    {
                        CreateServiceHost();

                        _serviceHost.Open();
                        _serviceStarted = true;

                        _logger.InfoFormat("{0} has restarted the PingService servicehost.", request.Nick);
                        resultMessage = "PingService started.";
                    }
                }

                // Someone is trying to stop our servicehost...
                if (message[1].Equals("stop", StringComparison.OrdinalIgnoreCase))
                {
                    // Cap'n, she's already a stopped! We kinna do this!
                    if (!_serviceStarted)
                    {
                        _logger.WarnFormat("{0} is attempting to stop the PingService servicehost, but it is already stopped.", request.Nick);
                        resultMessage = "Failed attempting to stop the PingService servicehost, it's already stopped.";                        
                    }
                    else
                    {
                        _serviceHost.Close();
                        _serviceStarted = false;

                        _logger.InfoFormat("{0} has stopped the PingService servicehost.", request.Nick);
                        resultMessage = "PingService stopped.";
                    }
                }

                // They're asking for the status of the service.
                if (message[1].Equals("status", StringComparison.OrdinalIgnoreCase))
                {
                    resultMessage = String.Format("The PingHost service is currently {0}.", _serviceStarted ? "started" : "stopped");
                }

                ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Public, resultMessage));
            }
        }

        /// <summary>
        /// A private utility method that creates and binds our service endpoint.
        /// </summary>
        private void CreateServiceHost()
        {
            _serviceHost = new ServiceHost(typeof(PingService), _baseAddress);
            _serviceHost.AddServiceEndpoint(typeof(IPingService), new BasicHttpBinding(), "PingService");
            _serviceHost.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });
        }
    }
}
