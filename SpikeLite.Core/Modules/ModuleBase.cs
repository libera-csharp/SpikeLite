/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using log4net.Ext.Trace;
using SpikeLite.Communications;
using SpikeLite.Communications.Irc.Configuration;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Modules
{
    /// <summary>
    /// An abstract implementation of things that a module might do. We have some lovely wiring in here,
    /// some registration with our module manager (yes, it's still a little coupled) and some virtual points
    /// for being overridden down the line.
    /// </summary>
    public abstract class ModuleBase : IModule
    {
        #region Plumbing Properties

        /// <summary>
        /// Holds our log4net logger.
        /// </summary>
        private TraceLogImpl _logger;

        /// <summary>
        /// This property exposes the module container to implementations of this module.
        /// </summary>
        public ModuleManager ModuleManagementContainer { get; set; }

        /// <summary>
        /// Gets the Log4NET logger that we're using.
        /// </summary>
        protected virtual TraceLogImpl Logger
        {
            get { return _logger ?? (_logger = (TraceLogImpl) TraceLogManager.GetLogger(GetType())); }
        }

        #endregion 

        #region IModule Implementation

        #region Module Information Properties

        public ModuleBase()
        {
            RequiredAccessLevel = AccessLevel.Public;
        }

        /// <summary>
        /// Gets or sets the name of the associated module. This would be something like "Google search module."
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the associated module. This would be something like "Searches the internet."
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a short summary of usage instructions. This would be something like "~google [topic]."
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AccessLevel"/> required to run the module.
        /// </summary>
        /// 
        /// <remarks>
        /// If no value is supplied, a default access level of Public is assumed.
        /// </remarks>
        public AccessLevel RequiredAccessLevel { get; set; }

        // TODO: Kog 12/26/2008 - This totally sucks, we need to replace this idea. Messages should most likely
        // TODO:                  have some sort of information as to what network they were received on, so we can
        // TODO:                  pull this in the proper place... for now let's just kludge.

        /// <summary>
        /// Gets or sets our network information, including the severs and channels.
        /// </summary>
        public Network NetworkConnectionInformation { get; set; }

        #endregion

        /// <summary>
        /// Given from our contract in IModule. This class does not actually provide any implementation
        /// details.
        /// </summary>
        /// 
        /// <param name="request">A message relayed to this module.</param>
        /// 
        /// <remarks>
        /// As filtering on a per-message basis is done by any given module (in theory to allow N consumers),
        /// every module is sent every message, and they can either handle or disregard the message. There is 
        /// currently no mechanism for stopping propagation of the message (IE: we've handled the message). This
        /// may come when we hook up AOP (then we'll expect a 1:1 between message and consumer, sans advice points).
        /// </remarks>
        public abstract void HandleRequest(Request request);

        protected void Reply(Request request, ResponseTargetType responseTargetType, string responseMessage)
        {
            ModuleManagementContainer.HandleResponse(request.CreateResponse(responseTargetType, ResponseType.Message, responseMessage));
        }

        protected void Reply(Request request, ResponseType responseType, string responseMessage)
        {
            ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseTargetType.Public, responseType, responseMessage));
        }

        protected void Reply(Request request, ResponseTargetType responseTargetType, string responseMessage, params object[] formatObjects)
        {
            ModuleManagementContainer.HandleResponse(request.CreateResponse(responseTargetType, ResponseType.Message, responseMessage, formatObjects));
        }

        protected void Reply(Request request, ResponseType responseType, string responseMessage, params object[] formatObjects)
        {
            ModuleManagementContainer.HandleResponse(request.CreateResponse(ResponseTargetType.Public, responseType, responseMessage, formatObjects));
        }

        /// <summary>
        /// Provides a utility method for responding to users, cutting down on copy/paste.
        /// </summary>
        /// 
        /// <param name="request">The request that triggered the message.</param>
        /// <param name="responseType">The type of response to return: whether the message is public or private.<see cref="ResponseTargetType"/></param>
        /// <param name="responseMessage">The literal text of the response message.</param>
        /// <param name="formatObjects">An optional set of format objects. Will be passed along to String.format.</param>
        /// 
        /// <returns></returns>
        protected void Reply(Request request, ResponseTargetType responseTargetType, ResponseType responseType, string responseMessage, params object[] formatObjects)
        {
            ModuleManagementContainer.HandleResponse(request.CreateResponse(responseTargetType, responseType, responseMessage, formatObjects));
        }

        #endregion
    }
}