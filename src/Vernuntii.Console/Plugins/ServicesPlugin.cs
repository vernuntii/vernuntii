using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Represents the main entry point for dependency injection.
    /// </summary>
    public class ServicesPlugin : Plugin, IServicesPlugin
    {
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly ILogger _logger;
        private readonly IVersionCacheCheckPlugin _cacheCheckPlugin;
        private IServiceProvider? _serviceProvider;

        public ServicesPlugin(IVersionCacheCheckPlugin cacheCheckPlugin, ILogger<ServicesPlugin> logger)
        {
            _cacheCheckPlugin = cacheCheckPlugin ?? throw new ArgumentNullException(nameof(cacheCheckPlugin));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async ValueTask OnCreateServiceProvider()
        {
            if (_cacheCheckPlugin.IsCacheUpToDate) {
                return;
            }

            var services = _services;
            await Events.FulfillAsync(ServicesEvents.ConfigureServices, services);
            var globalServices = services.BuildServiceProvider();
            AddDisposable(globalServices);
            _serviceProvider = globalServices;
            _logger.LogTrace("Created global service provider");
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            Events.Every(ServicesEvents.CreateServiceProvider)
                //.Where(_ => !_cacheCheckPlugin.IsCacheUpToDate)
                .Subscribe(_ => OnCreateServiceProvider());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IServiceProvider GetServiceProvider() => _serviceProvider
            ?? throw new InvalidOperationException($"To access the services, the event \"{nameof(ServicesEvents.CreateServiceProvider)}\" of \"{nameof(ServicesEvents)}\" must be called first");

        /// <inheritdoc cref="IServiceProvider.GetService(Type)"/>
        public object? GetService(Type serviceType) =>
            GetServiceProvider().GetService(serviceType);
    }
}
