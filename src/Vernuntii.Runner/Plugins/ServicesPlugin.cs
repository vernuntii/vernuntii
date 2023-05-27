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
        private readonly IPluginRegistry _pluginRegistry;
        private readonly ILogger _logger;
        private IServiceProvider? _serviceProvider;

        public ServicesPlugin(IPluginRegistry pluginRegistry, ILogger<ServicesPlugin> logger)
        {
            _pluginRegistry = pluginRegistry;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private Task OnCreateServiceProvider()
        {
            if (_pluginRegistry.TryGetPlugin<IVersionCachePlugin>(out var versionCachePlugin) && versionCachePlugin.IsCacheUpToDate) {
                return Task.CompletedTask;
            }

            var services = _services;
            var task = Events.EmitAsync(ServicesEvents.OnConfigureServices, services);

            void CreateServiceBuilder()
            {
                var serviceProvider = services.BuildServiceProvider();
                AddDisposable(serviceProvider);
                _serviceProvider = serviceProvider;
                _logger.LogTrace("Created global service provider");
            }

            async Task CreateServiceBuilderAsync()
            {
                await task.ConfigureAwait(false);
                CreateServiceBuilder();
            }

            if (task.IsCompletedSuccessfully) {
                CreateServiceBuilder();
                return Task.CompletedTask;
            } else {
                return CreateServiceBuilderAsync();
            }
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            // We only want once create the global service provider
            Events.OneTime(LifecycleEvents.BeforeEveryRun)
                .And(ServicesEvents.CreateServiceProvider)
                .Subscribe(OnCreateServiceProvider);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IServiceProvider GetServiceProvider() => _serviceProvider
            ?? throw new InvalidOperationException($"To access the services, the event \"{nameof(ServicesEvents.CreateServiceProvider)}\" of \"{nameof(ServicesEvents)}\" must be called first");

        /// <inheritdoc cref="IServiceProvider.GetService(Type)"/>
        public object? GetService(Type serviceType) =>
            GetServiceProvider().GetService(serviceType);
    }
}
