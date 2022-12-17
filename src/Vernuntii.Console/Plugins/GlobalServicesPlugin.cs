using System.Runtime.CompilerServices;
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
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly ILogger _logger;
        private readonly IVersionCacheCheckPlugin _cacheCheckPlugin;
        private IServiceProvider? _serviceProvider;

        public GlobalServicesPlugin(IVersionCacheCheckPlugin cacheCheckPlugin, ILogger<GlobalServicesPlugin> logger)
        {
            _cacheCheckPlugin = cacheCheckPlugin ?? throw new ArgumentNullException(nameof(cacheCheckPlugin));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void OnCreateServiceProvider()
        {
            var services = _services;
            Events.FireEvent(GlobalServicesEvents.ConfigureServices, services);
            var globalServices = services.BuildServiceProvider();
            AddDisposable(globalServices);
            _serviceProvider = globalServices;
            _logger.LogTrace("Created global service provider");
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            Events.OnNextEvent(
                GlobalServicesEvents.CreateServiceProvider,
                _ => OnCreateServiceProvider(),
                () => !_cacheCheckPlugin.IsCacheUpToDate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IServiceProvider GetServiceProvider() => _serviceProvider
            ?? throw new InvalidOperationException($"To access the services, the event \"{nameof(GlobalServicesEvents.CreateServiceProvider)}\" of \"{nameof(GlobalServicesEvents)}\" must be called first");

        /// <inheritdoc cref="IServiceProvider.GetService(Type)"/>
        public object? GetService(Type serviceType) =>
            GetServiceProvider().GetService(serviceType);
    }
}
