/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.Communications;
using SpikeLite.Communications.IRC;
using SpikeLite.Persistence;
using log4net;

namespace SpikeLite.Modules
{
    /// <summary>
    /// An abstract implementation of things that a module might do. We have some lovely wiring in here,
    /// some registration with our module manager (yes, it's still a little coupled) and some virtual points
    /// for being overridden down the line.
    /// </summary>
    public abstract class ModuleBase : IModule
    {
        /// <summary>
        /// Holds our log4net logger.
        /// </summary>
        private ILog _logger;

        /// <summary>
        /// This property exposes the module container to implementations of this module.
        /// </summary>
        public ModuleManager ModuleManagementContainer { get; set; }

        /// <summary>
        /// Gets the Log4NET logger that we're using.
        /// </summary>
        protected virtual ILog Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = LogManager.GetLogger(typeof(ModuleBase));
                }

                return _logger;
            }
        }

        // TODO: Kog 12/25/2008 replace this with inheritence - have a "licensed module" or something.

        /// <summary>
        /// Gets or sets an optional API or license key that a module may require.
        /// </summary>
        public virtual string ApiKey
        {
            get; set;
        }

        // TODO: Kog 12/26/2008 - This totally sucks, we need to replace this idea. Messages should most likely
        // TODO:                  have some sort of information as to what network they were received on, so we can
        // TODO:                  pull this in the proper place... for now let's just kludge.

        /// <summary>
        /// Gets or sets our network information, including the severs and channels.
        /// </summary>
        public Network NetworkConnectionInformation
        {
            get; set;
        }

        /// <summary>
        /// We've received a message, let's see if we can handle it.
        /// </summary>
        /// 
        /// <param name="request">The message we've received.</param>
        /// 
        /// <remarks>
        /// This was originally added to support the tell module, but for now we're sorta half-assing that.
        /// While we should probably get rid of this, I have a feeling that we'll probably need this soon so 
        /// I left it in. Delete if you'd like.
        /// </remarks>
        public void HandleRequest(Request request)
        {
            InternalHandleRequest(request);
        }

        /// <summary>
        /// Provide a method that modules can implement and has the wiring already done by HandleRequest.
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
        protected abstract void InternalHandleRequest(Request request);
    }
}