﻿using DotNetUtils.DependencyInjection.Builders.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetUtils.DependencyInjection
{
    public abstract class ServiceConfiguration<TServiceProvider>
        where TServiceProvider : ServiceProvider<TServiceProvider>
    {
        #region Public Fields
        /// <summary>
        /// The xxHash of the <see cref="ServiceConfiguration.Name"/>
        /// </summary>
        public readonly UInt32 Id;

        /// <summary>
        /// The primary lookup key for the current service.
        /// </summary>
        public readonly String Name;

        /// <summary>
        /// The bound <see cref="TypeFactory"/>.
        /// </summary>
        public readonly TypeFactory<TServiceProvider> TypeFactory;

        /// <summary>
        /// The service lifetime.
        /// </summary>
        public readonly ServiceLifetime Lifetime;

        /// <summary>
        /// An array of strings with which this service will be cached once activated.
        /// All queries matching any of these values will return the defined
        /// configuration.
        /// </summary>
        public readonly String[] CacheNames;

        /// <summary>
        /// An array of ids with which this service will be cached once activated.
        /// All queries matching any of these values will return the defined
        /// configuration.
        /// </summary>
        public readonly UInt32[] CacheIds;

        /// <summary>
        /// An array of actions to preform when building a new instace
        /// </summary>
        public readonly CustomAction<TServiceProvider, ServiceConfiguration<TServiceProvider>, IServiceConfigurationBuilder<TServiceProvider>>[] Setups;
        #endregion

        #region Constructors
        internal ServiceConfiguration(
            String name,
            TypeFactory<TServiceProvider> typeFactory,
            ServiceLifetime lifetime,
            String[] cacheNames,
            CustomAction<TServiceProvider, ServiceConfiguration<TServiceProvider>, IServiceConfigurationBuilder<TServiceProvider>>[] setups)
        {
            this.Id = name.xxHash();
            this.Name = name;
            this.TypeFactory = typeFactory;
            this.Lifetime = lifetime;
            this.CacheNames = cacheNames;
            this.CacheIds = this.CacheNames.Select(cn => cn.xxHash()).ToArray();
            this.Setups = setups;
        }
        #endregion

        #region Helper Methods
        public abstract ServiceConfigurationManager<TServiceProvider> BuildServiceCofigurationManager(TServiceProvider provider);

        public virtual Object GetInstance(TServiceProvider provider)
        {
            return provider.GetServiceConfigurationManager(this).GetInstance();
        }

        public virtual Object GetInstance(TServiceProvider provider, Action<Object, TServiceProvider, ServiceConfiguration<TServiceProvider>> customSetup, Int32 customSetupOrder)
        {
            return provider.GetServiceConfigurationManager(this).GetInstance(customSetup, customSetupOrder);
        }
        #endregion

    }
}
