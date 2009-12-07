/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */


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
    public class WCFPingServiceHostModule : ModuleBase
    {
        /// <summary>
        /// Stores our log4net logger.
        /// </summary>
        private static readonly TraceLogImpl _logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(WCFPingServiceHostModule));

        /// <summary>
        /// Register our WCF endpoint.
        /// </summary>
        /// 
        ///
        /// <remarks>
        /// Users on Vista or greater might have to reserve the endpoint via Netsh.exe as follows:
        /// 
        /// <code>
        /// netsh http add urlacl url=http://+:8080/SpikeLite/Ping user=DOMAIN\user 
        /// </code>  
        /// 
        /// Please make sure to run your cmd session with administrative rights.
        /// </remarks>
        public WCFPingServiceHostModule()
        {
            Uri baseAddress = new Uri("http://localhost:8080/SpikeLite/Ping");
            ServiceHost selfHost = new ServiceHost(typeof(WcfPingService), baseAddress);

            try
            {
                selfHost.AddServiceEndpoint(typeof(IWcfPingService), new BasicHttpBinding(), "WcfPingService");

                ServiceMetadataBehavior smb = new ServiceMetadataBehavior { HttpGetEnabled = true };
                selfHost.Description.Behaviors.Add(smb);

                selfHost.Open();
            }
            catch (CommunicationException ce)
            {
                _logger.InfoFormat("Caught an exception trying to bind our WCF endpoint: {0}", ce);
            }     
        }

        public override void HandleRequest(Request request)
        {
            //No-Op for now.
        }
    }
}
