/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;
using SpikeLite.Modules;

namespace SpikeLite.IPC.WebHost
{
    /// <summary>
    /// This class provides a prototyping module, allowing people to write WCF services that can be hosted against the bot. The services for
    /// this particular prototype are set up to do SOAP hosting.
    /// </summary>
    /// 
    /// <typeparam name="C">The concrete type of our service implementation, such that it is an implementation of <typeparamref name="C"/>.</typeparam>
    /// <typeparam name="I">Our service interface, fully decorated with the appropriate <see cref="ServiceContractAttribute"/> et al.</typeparam>
    public class ServiceHostModule<I, C> : ModuleBase where C : I
    {
        #region Data Members

        /// <summary>
        /// Holds a reference to the <see cref="ServiceHost"/> we're going to use to host our WS endpoints.
        /// </summary>
        private ServiceHost _serviceHost;

        /// <summary>
        /// Holds a flag, representing whether or not our WCF <see cref="ServiceHost"/> has been started. This allows us to do things like
        /// starting and stopping service.
        /// </summary>
        private bool _serviceStarted;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the binding address for this service.
        /// </summary>
        public string ServiceAddress { get; set; }

        #endregion

        #region Init Factory Methods

        /// <summary>
        /// Binds our WCF endpoint for the given HTTP setup, using <see cref="BasicHttpBinding"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// Users on Vista or greater might have to reserve the endpoint via Netsh.exe as follows:
        /// 
        /// <code>
        /// netsh http add urlacl url=http://+:[port]/[path/to/service] user=DOMAIN\user 
        /// </code>  
        /// 
        /// Please make sure to run your cmd session with administrative rights. Users on Mono should note that
        /// unlike windows, which seems to actually allow any host (via using + instead of a literal host), Mono on Linux
        /// seems to be quite specific on what path it binds to.
        /// </remarks>
        public void InitModule()
        {
            try
            {
                _serviceHost = CreateServiceHost();

                Logger.InfoFormat("Opening servicehost for {0} via URI {1}", Name, ServiceAddress);

                _serviceHost.Open();
                _serviceStarted = true;
            }
            catch (CommunicationException ce)
            {
                Logger.WarnFormat("Caught an exception trying to bind our WCF endpoint: {0}", ce);
            }
        }

        #endregion

        #region Service Status Message Handling (start, stop, status etc).

        public override void HandleRequest(Request request)
        {
            string[] message = request.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (request.RequestFrom.AccessLevel >= AccessLevel.Root &&
                message[0].Equals(String.Format("~{0}", Name), StringComparison.OrdinalIgnoreCase) &&
                message.Length == 2)
            {
                string resultMessage = String.Format("Unknown {0} operation. Please specify one of the following operations: start, stop, status.", Name);

                // Someone is attempting to start our servicehost...
                if (message[1].Equals("start", StringComparison.OrdinalIgnoreCase))
                {
                    // Naughty zoot, don't double-start.
                    if (_serviceStarted)
                    {
                        Logger.WarnFormat("{0} is attempting to start the {1} servicehost, but it is already started.", request.Nick, Name);
                        resultMessage = String.Format("Failed attempting to start the {0} servicehost, it's already started.", Name);
                    }
                    else
                    {
                        _serviceHost = CreateServiceHost();

                        _serviceHost.Open();
                        _serviceStarted = true;

                        Logger.InfoFormat("{0} has restarted the {1} servicehost.", request.Nick, Name);
                        resultMessage = String.Format("{0} started.", Name);
                    }
                }

                // Someone is trying to stop our servicehost...
                if (message[1].Equals("stop", StringComparison.OrdinalIgnoreCase))
                {
                    // Cap'n, she's already a stopped! We kinna do this!
                    if (!_serviceStarted)
                    {
                        Logger.WarnFormat("{0} is attempting to stop the {1} servicehost, but it is already stopped.", request.Nick, Name);
                        resultMessage = String.Format("Failed attempting to stop the {0} servicehost, it's already stopped.", Name);
                    }
                    else
                    {
                        _serviceHost.Close();
                        _serviceStarted = false;

                        Logger.InfoFormat("{0} has stopped the {1} servicehost.", request.Nick, Name);
                        resultMessage = String.Format("{0} stopped.", Name);
                    }
                }

                // They're asking for the status of the service.
                if (message[1].Equals("status", StringComparison.OrdinalIgnoreCase))
                {
                    resultMessage = String.Format("The {0} servicehost is currently {1}.", Name, _serviceStarted ? "started" : "stopped");
                }

                ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseType.Public, resultMessage));
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// A private utility method that creates and binds our service endpoint.
        /// </summary>
        private ServiceHost CreateServiceHost()
        {
            ServiceHost serviceHost = new ServiceHost(typeof(C), new Uri(ServiceAddress));
            serviceHost.AddServiceEndpoint(typeof(I), new BasicHttpContextBinding(), Name);
            serviceHost.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });

            return serviceHost;
        }

        #endregion
    }
}
