/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using SpikeLite.Communications;
using SpikeLite.Persistence;
using SpikeLite.Configuration;

namespace SpikeLite.Modules
{
    public abstract class ModuleBase : IModule
    {
        #region Fields

        /// <summary>
        /// Contains a reference to our configuration container. This is handed to us at load time.
        /// </summary>
        private readonly SpikeLiteSection _configuration;

        /// <summary>
        /// Contains a reference to our persistence layer.
        /// </summary>
        /// 
        /// <remarks>
        /// Kog - 10/19/2008: this sucks and we definitely need to get rid of it.
        /// </remarks>
        private PersistenceLayer _persistenceLayer;

        /// <summary>
        /// Contains a reference to the module manager that holds us. This allows us to pass messages
        /// back to the bot.
        /// </summary>
        /// 
        /// <remarks>
        /// Kog - 10/19/2008: this mechanism needs refactoring.
        /// </remarks>
        private ModuleManager _moduleManager;

        #endregion

        #region Properties

        /// <summary>
        /// This exposes the configuration container to implementations of this module. This property
        /// is immutable.
        /// </summary>
        protected virtual SpikeLiteSection Configuration
        {
            get { return _configuration; }
        }

        /// <summary>
        /// This exposes the persistence layer to implementations of this module. This property is immutable.
        /// </summary>
        protected virtual PersistenceLayer Persistence
        {
            get { return _persistenceLayer; }
        }

        /// <summary>
        /// This property exposes the module container to implementations of this module. This property is immutable.
        /// </summary>
        protected virtual ModuleManager ModuleManagementContainer
        {
            get { return _moduleManager; }
        }

        #endregion

        protected ModuleBase()
        {
            _configuration = SpikeLiteSection.GetSection();
        }

        /// <summary>
        /// Spin the module up. We take care of some some wiring here, and then call InternalInitModule. This
        /// method is not virtual as you are expected to override the internal variety.
        /// </summary>
        /// 
        /// <param name="moduleManager">Our module container.</param>
        /// <param name="persistenceLayer">Our persistence layer.</param>
        public void InitModule(ModuleManager moduleManager, PersistenceLayer persistenceLayer)
        {
            _persistenceLayer = persistenceLayer;
            _moduleManager = moduleManager;

            InternalInitModule();
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

        #region Internal Implementation Details

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

        /// <summary>
        /// Provide a default no-op method that gets called when we init the module. This allows us to do things
        /// like wire up DAOs since we have no IoC container at the moment.
        /// </summary>
        /// 
        /// <remarks>
        /// This can most likely be replaced with an event if we so desire, as can many of the other methods
        /// in this class.
        /// </remarks>
        protected virtual void InternalInitModule() { }

        #endregion 
    }
}