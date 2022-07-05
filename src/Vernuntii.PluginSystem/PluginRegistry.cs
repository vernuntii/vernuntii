using Microsoft.Extensions.Logging;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// The plugin registry.
    /// </summary>
    public sealed class PluginRegistry : IPluginRegistry, ISealed, IDisposable
    {
        /// <inheritdoc/>
        public IReadOnlyCollection<IPluginRegistration> PluginRegistrations => _pluginRegistrations.Sorted;

        bool ISealed.IsSealed => _isSealed;

        private PluginRegistrationCollection _pluginRegistrations;
        private int _pluginRegistrationCounter;
        private bool _isSealed;
        private readonly ILogger<PluginRegistry> _logger;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public PluginRegistry(ILogger<PluginRegistry> logger)
        {
            _pluginRegistrations = new PluginRegistrationCollection(this);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        private async ValueTask<IPluginRegistration> RegisterAsyncCore(Type serviceType, IPlugin plugin)
        {
            ((ISealed)this).EnsureNotSealed();
            var acceptRegistration = await plugin.OnRegistration(this);

            int pluginId;

            if (acceptRegistration) {
                pluginId = _pluginRegistrationCounter++;
            } else {
                pluginId = -1;
            }

            var pluginRegistration = new PluginRegistration(pluginId, serviceType, plugin);

            if (acceptRegistration) {
                _pluginRegistrations.Add(pluginRegistration);
                _logger.LogTrace("Accepted plugin registration: {ServiceType} ({PluginType})", pluginRegistration.ServiceType, pluginRegistration.PluginType);
            } else {
                _logger.LogTrace("Denied plugin registration: {ServiceType} ({PluginType})", pluginRegistration.ServiceType, pluginRegistration.PluginType);
            }

            return pluginRegistration;
        }

        /// <inheritdoc/>
        public async ValueTask<IPluginRegistration> RegisterAsync(Type serviceType, IPlugin plugin)
        {
            var pluginType = plugin.GetType();

            if (!serviceType.IsAssignableFrom(plugin.GetType())) {
                throw new ArgumentException($"Service type \"{serviceType}\" is not a subset of plugin type \"{pluginType.GetType()}\"", nameof(serviceType));
            }

            return await RegisterAsyncCore(serviceType, plugin);
        }

        /// <summary>
        /// Seals the registry.
        /// </summary>
        public void Seal()
        {
            _pluginRegistrations.Seal();
            _isSealed = true;
        }

        /// <inheritdoc/>
        public T First<T>()
            where T : IPlugin
        {
            ((ISealed)this).EnsureSealed();

            if (!_pluginRegistrations.AcendedPlugins.TryGetValue(typeof(T), out var firstPlugin)
                || firstPlugin is not T firstPluginTyped) {
                throw new PluginNotFoundException($"A plugin registered as service \"{typeof(T)}\" was not registered");
            }

            return firstPluginTyped;
        }

        /// <inheritdoc/>
        public void Dispose() { }

        private class Disposable : IDisposable
        {
            Action _action;

            public Disposable(Action action) => _action = action;

            public void Dispose() => _action();
        }
    }
}
