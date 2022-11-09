using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.PluginSystem.Lifecycle;
using Vernuntii.PluginSystem.Meta;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// The plugin registry.
    /// </summary>
    public sealed class PluginRegistry : IPluginRegistry, ISealed, IAsyncDisposable
    {
        /// <inheritdoc/>
        public IReadOnlyCollection<IPluginRegistration> PluginRegistrations => _pluginRegistrations.Ordered;

        bool ISealed.IsSealed => _isSealed;

        private ISealed _seal;
        private SemaphoreSlim _registationLock;
        private PluginDescriptorCollection _pluginDescriptors;
        private PluginRegistrations _pluginRegistrations;
        private ServiceProvider? _pluginProvider;
        private int _pluginRegistrationCounter;
        private bool _isSealed;
        private readonly ILogger<PluginRegistry> _logger;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public PluginRegistry(ILogger<PluginRegistry> logger)
        {
            _seal = this;
            _registationLock = new SemaphoreSlim(1);
            _pluginDescriptors = new PluginDescriptorCollection();
            _pluginRegistrations = new PluginRegistrations(this);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void DescribePluginRegistrationCore(PluginDescriptor pluginDescriptor) =>
            _pluginDescriptors.Add(pluginDescriptor);

        /// <inheritdoc/>
        public void DescribePluginRegistration(PluginDescriptor pluginDescriptor)
        {
            _registationLock.Wait();

            try {
                DescribePluginRegistrationCore(pluginDescriptor);
            } finally {
                _registationLock.Release();
            }
        }

        private async ValueTask ConstructPluginsAsync()
        {
            var pluginDescriptors = _pluginDescriptors;
            var pluginProvider = pluginDescriptors.BuildServiceProvider();
            var pluginsWithRegistrationAspect = new List<IPluginRegistrationAspect>();

            foreach (var pluginDescriptor in pluginDescriptors) {
                var plugin = (IPlugin)pluginProvider.GetRequiredService(pluginDescriptor.ServiceType);
                bool acceptRegistration;

                if (plugin is IPluginRegistrationAspect pluginWithRegistrationAspect) {
                    pluginsWithRegistrationAspect.Add(pluginWithRegistrationAspect);
                    acceptRegistration = await pluginWithRegistrationAspect.OnRegistration(this);
                } else {
                    acceptRegistration = true;
                }

                var pluginId = acceptRegistration
                    ? _pluginRegistrationCounter++
                    : -1;

                if (acceptRegistration) {
                    var pluginDescriptorWithInstance = new PluginDescriptorBase<IPlugin>(
                        pluginDescriptor.ServiceType,
                        plugin,
                        pluginDescriptor.PluginType);

                    var pluginRegistration = new PluginRegistration(pluginId, pluginDescriptorWithInstance);

                    _pluginRegistrations.Add(pluginRegistration);
                    _logger.LogTrace("Accepted plugin registration: {ServiceType} ({PluginType})", pluginRegistration.ServiceType, pluginRegistration.ImplementationType);
                } else {
                    _logger.LogTrace("Denied plugin registration: {ServiceType} ({PluginType})", pluginDescriptor.ServiceType, pluginDescriptor.PluginType);
                }
            }

            _pluginProvider = pluginProvider;
        }

        /// <summary>
        /// Completes the registation phase by loading plugin dependencies and constructing missing plugins.
        /// </summary>
        public async ValueTask CompleteRegistrationPhase()
        {
            await _registationLock.WaitAsync();

            try {
                _seal.ThrowIfSealed();
                await ConstructPluginsAsync();
                Seal();
            } finally {
                _registationLock.Release();
            }

            void Seal()
            {
                _pluginRegistrations.Seal();
                _isSealed = true;
            }
        }

        /// <inheritdoc/>
        public T GetPlugin<T>()
            where T : IPlugin
        {
            _seal.ThrowIfNotSealed();

            if (!_pluginRegistrations.FirstByServiceType.TryGetValue(typeof(T), out var plugin)
                || plugin is not T typedPlugin) {
                throw new PluginNotFoundException($"A plugin with service type of \"{typeof(T)}\" was not registered");
            }

            return typedPlugin;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync() =>
            _pluginProvider?.DisposeAsync() ?? ValueTask.CompletedTask;

        private class Disposable : IDisposable
        {
            Action _action;

            public Disposable(Action action) => _action = action;

            public void Dispose() => _action();
        }
    }
}
