﻿/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

// TODO: Kog 12/8/2009 - Refactor this into a prototype... right now it's just pure copy and paste. But then, it's also 0500 on Tuesday. And damnit, this is a branch anyway.

using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using log4net.Ext.Trace;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;
using SpikeLite.Modules;

namespace SpikeLite.IPC.WebHost.Services
{
    public class MessagingServiceHostModule : ModuleBase
    {
        #region Data Members

        /// <summary>
        /// Stores our log4net logger.
        /// </summary>
        private readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(MessagingServiceHostModule));

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
        private readonly Uri _baseAddress = new Uri("http://localhost:8081/SpikeLite/Messaging");

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
        /// netsh http add urlacl url=http://+:8081/SpikeLite/Messaging user=DOMAIN\user 
        /// </code>  
        /// 
        /// Please make sure to run your cmd session with administrative rights.
        /// </remarks>
        public MessagingServiceHostModule()
        {
            try
            {
                CreateServiceHost();

                _logger.InfoFormat("Opening servicehost for MessagingServiceHost via URI {0}", _baseAddress.AbsolutePath);

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
                message[0].Equals("~messaginghost", StringComparison.OrdinalIgnoreCase) &&
                message.Length == 2)
            {
                string resultMessage = "Unknown messaginghost operation. Please specify one of the following operations: start, stop, status.";

                // Someone is attempting to start our servicehost...
                if (message[1].Equals("start", StringComparison.OrdinalIgnoreCase))
                {
                    // Naughty zoot, don't double-start.
                    if (_serviceStarted)
                    {
                        _logger.WarnFormat("{0} is attempting to start the Messaging servicehost, but it is already started.", request.Nick);
                        resultMessage = "Failed attempting to start the Messaging servicehost, it's already started.";
                    }
                    else
                    {
                        CreateServiceHost();

                        _serviceHost.Open();
                        _serviceStarted = true;

                        _logger.InfoFormat("{0} has restarted the Messaging servicehost.", request.Nick);
                        resultMessage = "Messaging started.";
                    }
                }

                // Someone is trying to stop our servicehost...
                if (message[1].Equals("stop", StringComparison.OrdinalIgnoreCase))
                {
                    // Cap'n, she's already a stopped! We kinna do this!
                    if (!_serviceStarted)
                    {
                        _logger.WarnFormat("{0} is attempting to stop the Messaging servicehost, but it is already stopped.", request.Nick);
                        resultMessage = "Failed attempting to stop the Messaging servicehost, it's already stopped.";
                    }
                    else
                    {
                        _serviceHost.Close();
                        _serviceStarted = false;

                        _logger.InfoFormat("{0} has stopped the Messaging servicehost.", request.Nick);
                        resultMessage = "Messaging stopped.";
                    }
                }

                // They're asking for the status of the service.
                if (message[1].Equals("status", StringComparison.OrdinalIgnoreCase))
                {
                    resultMessage = String.Format("The Messaging service is currently {0}.", _serviceStarted ? "started" : "stopped");
                }

                ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Public, resultMessage));
            }
        }

        /// <summary>
        /// A private utility method that creates and binds our service endpoint.
        /// </summary>
        private void CreateServiceHost()
        {
            _serviceHost = new ServiceHost(typeof(MessagingService), _baseAddress);
            _serviceHost.AddServiceEndpoint(typeof(IMessagingService), new BasicHttpBinding(), "MessagingService");
            _serviceHost.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });
        }
    }
}
