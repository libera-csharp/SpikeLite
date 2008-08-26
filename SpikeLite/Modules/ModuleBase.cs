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

        private SpikeLiteSection _configuration;
        private PersistenceLayer _persistenceLayer;
        private ModuleManager _moduleManager;

        protected IModule _nextModule;

        #endregion

        public ModuleBase()
        {
            _configuration = SpikeLiteSection.GetSection();
        }

        #region IModule Implementations

        protected abstract void InternalHandleRequest(Request request);

        public void InitModule(ModuleManager moduleManager, PersistenceLayer persistenceLayer)
        {
            _persistenceLayer = persistenceLayer;
            _moduleManager = moduleManager;
        }

        public void HandleRequest(Request request)
        {
            InternalHandleRequest(request);

            if (_nextModule != null)
            {
                _nextModule.HandleRequest(request);
            }
        }

        #endregion

        #region Awesome Protected Getters

        protected virtual SpikeLiteSection Configuration
        {
            get { return _configuration; }
        }

        protected virtual PersistenceLayer Persistence
        {
            get { return _persistenceLayer; }
        }

        protected virtual ModuleManager ModuleManagementContainer
        {
            get { return _moduleManager; }
        }

        #endregion
    }
}