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

        private Task OnCreateServiceProvider()
        {
            if (_cacheCheckPlugin.IsCacheUpToDate) {
                return Task.CompletedTask;
            }

            var services = _services;
            var task = Events.FulfillAsync(ServicesEvents.ConfigureServices, services);

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
            Events.Every(ServicesEvents.CreateServiceProvider)
                .Subscribe(OnCreateServiceProvider)
                .DisposeWhenDisposing(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IServiceProvider GetServiceProvider() => _serviceProvider
            ?? throw new InvalidOperationException($"To access the services, the event \"{nameof(ServicesEvents.CreateServiceProvider)}\" of \"{nameof(ServicesEvents)}\" must be called first");

        /// <inheritdoc cref="IServiceProvider.GetService(Type)"/>
        public object? GetService(Type serviceType) =>
            GetServiceProvider().GetService(serviceType);
    }
}
