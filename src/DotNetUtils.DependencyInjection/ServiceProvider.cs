﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetUtils.DependencyInjection
{
    public partial class ServiceProvider<TServiceProvider> : IDisposable
        where TServiceProvider : ServiceProvider<TServiceProvider>
    {
        #region Private Fields
        /// <summary>
        /// All registered <see cref="ServiceConfiguration"/> instances by name.
        /// </summary>
        private Dictionary<String, ServiceConfiguration<TServiceProvider>> _registeredNamesServices;

        /// <summary>
        /// All <see cref="ServiceConfigurationManager"/> instances that have been activated
        /// for the current scope by name.
        /// </summary>
        private Dictionary<String, ServiceConfigurationManager<TServiceProvider>> _activeNamesServices;

        /// <summary>
        /// All registered <see cref="ServiceConfiguration"/> instances by id.
        /// </summary>
        private Dictionary<UInt32, ServiceConfiguration<TServiceProvider>> _registeredIdsServices;

        /// <summary>
        /// All <see cref="ServiceConfigurationManager"/> instances that have been activated
        /// for the current scope by id.
        /// </summary>
        private Dictionary<UInt32, ServiceConfigurationManager<TServiceProvider>> _activeIdsServices;

        private Boolean _disposing;
        private TServiceProvider _parent;
        private List<TServiceProvider> _children;
        #endregion

        #region Public Fields
        /// <summary>
        /// Indicates that this provider is the Root most scope, from which all other scopes are children.
        /// When true, <see cref="SingletonServiceConfigurationManager"/> instances will all reside here.
        /// </summary>
        public readonly Boolean IsRoot;

        /// <summary>
        /// The rootmost ServiceProvider.
        /// </summary>
        public readonly TServiceProvider Root;
        #endregion

        #region Events
        public event OnEventDelegate<TServiceProvider, ServiceConfiguration<TServiceProvider>> OnServiceActivated;
        #endregion

        #region Constructors
        protected ServiceProvider(
            Dictionary<String, ServiceConfiguration<TServiceProvider>> services)
        {
            this.IsRoot = true;
            this.Root = this as TServiceProvider;

            _registeredNamesServices = services;
            _registeredIdsServices = services.Values.ToDictionaryByValue(sc => sc.Id);
            _activeNamesServices = new Dictionary<String, ServiceConfigurationManager<TServiceProvider>>();
            _activeIdsServices = new Dictionary<UInt32, ServiceConfigurationManager<TServiceProvider>>();

            _children = new List<TServiceProvider>();
        }
        internal ServiceProvider(TServiceProvider parent)
        {
            this.IsRoot = false;
            this.Root = parent.Root;

            _registeredNamesServices = parent._registeredNamesServices;
            _registeredIdsServices = parent._registeredIdsServices;
            _activeNamesServices = new Dictionary<String, ServiceConfigurationManager<TServiceProvider>>();
            _activeIdsServices = new Dictionary<UInt32, ServiceConfigurationManager<TServiceProvider>>();

            _parent = parent;
            _children = new List<TServiceProvider>();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Return the internal <see cref="ServiceConfigurationManager"/>, or create a new one
        /// if the <see cref="ServiceConfiguration"/> is not yet active.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        internal ServiceConfigurationManager<TServiceProvider> GetServiceConfigurationManager(ServiceConfiguration<TServiceProvider> configuration)
        {
            if(_activeNamesServices.TryGetValue(configuration.Name, out ServiceConfigurationManager<TServiceProvider> manager))
            {
                return manager;
            }

            return this.ActivateServiceConfiguration(configuration);
        }

        /// <summary>
        /// Activate a given <see cref="ServiceConfiguration"/>. This will create a new
        /// <see cref="ServiceConfigurationManager"/> instance and store its lookup type so
        /// that it will be utilized by all future queries of any 
        /// <see cref="ServiceConfiguration.CacheNames"/>.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private ServiceConfigurationManager<TServiceProvider> ActivateServiceConfiguration(ServiceConfiguration<TServiceProvider> configuration)
        {
            ServiceConfigurationManager<TServiceProvider> manager = configuration.BuildServiceCofigurationManager(this as TServiceProvider);

            // Cache all of the configurations CacheNames so that future lookup will return this instance.
            // If there is any overlap at all this will fail.
            foreach(String cacheName in configuration.CacheNames)
            {
                _activeNamesServices.Add(cacheName, manager);
            }

            // Cache all of the configurations CacheIds so that future lookup will return this instance.
            // If there is any overlap at all this will fail.
            foreach (UInt32 cacheId in configuration.CacheIds)
            {
                _activeIdsServices.Add(cacheId, manager);
            }

            this.OnServiceActivated?.Invoke(this as TServiceProvider, configuration);

            return manager;
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            if (_disposing)
                return;

            _disposing = true;

            this.Dispose(_disposing);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            foreach (TServiceProvider child in _children)
            {
                child.Dispose();
            }
                

            foreach (ServiceConfigurationManager<TServiceProvider> manager in _activeNamesServices.Values)
            {
                manager.Dispose();
            }

            _parent = default;
            _children.Clear();

            _activeNamesServices.Clear();
            _activeIdsServices.Clear();

            _registeredNamesServices.Clear();
            _registeredIdsServices.Clear();
        }
        #endregion
    }
}
