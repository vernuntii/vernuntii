using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Represents the global service collection.
    /// </summary>
    public class GlobalServicesPlugin : Plugin, IGlobalServicesPlugin
    {
        /// <inheritdoc/>
        public ServiceDescriptor this[int index] {
            get => ((IList<ServiceDescriptor>)_services)[index];
            set => ((IList<ServiceDescriptor>)_services)[index] = value;
        }

        /// <inheritdoc/>
        public int Count => _services.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => _services.IsReadOnly;

        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly ILogger _logger;
        private readonly IVersionCacheCheckPlugin _cacheCheckPlugin;

        public GlobalServicesPlugin(IVersionCacheCheckPlugin cacheCheckPlugin, ILogger<GlobalServicesPlugin> logger)
        {
            _cacheCheckPlugin = cacheCheckPlugin ?? throw new ArgumentNullException(nameof(cacheCheckPlugin));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void Add(ServiceDescriptor item) =>
            _services.Add(item);

        /// <inheritdoc/>
        public void Clear() =>
            _services.Clear();

        /// <inheritdoc/>
        public bool Contains(ServiceDescriptor item) =>
            _services.Contains(item);

        /// <inheritdoc/>
        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) =>
            _services.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public IEnumerator<ServiceDescriptor> GetEnumerator() =>
            _services.GetEnumerator();

        /// <inheritdoc/>
        public int IndexOf(ServiceDescriptor item) =>
            _services.IndexOf(item);

        /// <inheritdoc/>
        public void Insert(int index, ServiceDescriptor item) =>
            _services.Insert(index, item);

        /// <inheritdoc/>
        public bool Remove(ServiceDescriptor item) =>
            _services.Remove(item);

        /// <inheritdoc/>
        public void RemoveAt(int index) =>
            _services.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)_services).GetEnumerator();

        private void OnCreateServiceProvider()
        {
            var services = _services;
            Events.Publish(GlobalServicesEvents.ConfigureServices, services);
            var globalServices = AddDisposable(services.BuildLifetimeScopedServiceProvider());
            _logger.LogTrace("Created global service provider");
            Events.Publish(GlobalServicesEvents.CreatedServiceProvider, globalServices);
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            Events.SubscribeOnce(
                GlobalServicesEvents.CreateServiceProvider,
                _ => OnCreateServiceProvider(),
                () => !_cacheCheckPlugin.IsCacheUpToDate);
        }
    }
}
