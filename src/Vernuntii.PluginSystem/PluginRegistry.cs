namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// The plugin registry.
    /// </summary>
    public sealed class PluginRegistry : IPluginRegistry, IDisposable
    {
        /// <inheritdoc/>
        public IReadOnlyCollection<IPluginRegistration> PluginRegistrations => _pluginRegistrations;

        private List<Action<IPluginRegistration>> _consumePluginRegistrationActionList =
            new List<Action<IPluginRegistration>>();

        private SortedSet<PluginRegistration> _pluginRegistrations = new SortedSet<PluginRegistration>(PluginRegistrationComparer.Default);

        private int _pluginRegistrationCounter;
        private bool _isSealed;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public PluginRegistry()
        {
        }

        private void NotifyPluginRegistrationConsumer(Action<IPluginRegistration> consumePluginRegistrationAction)
        {
            foreach (var pluginRegistration in _pluginRegistrations) {
                consumePluginRegistrationAction(pluginRegistration);
            }
        }

        /// <inheritdoc/>
        public IDisposable AddPluginRegistrationConsumer(Action<IPluginRegistration> consumePluginRegistrationAction)
        {
            var disposable = new Disposable(() => _consumePluginRegistrationActionList.Remove(consumePluginRegistrationAction));
            _consumePluginRegistrationActionList.Add(consumePluginRegistrationAction);
            NotifyPluginRegistrationConsumer(consumePluginRegistrationAction);
            return disposable;
        }

        private void NotifyPluginRegistrationConsumers(IPluginRegistration pluginRegistration)
        {
            foreach (var consumePluginRegistrationAction in _consumePluginRegistrationActionList) {
                consumePluginRegistrationAction(pluginRegistration);
            }
        }

        /// <summary>
        /// Ensures that registry is not sealed.
        /// </summary>
        private void EnsureNotSealed()
        {
            if (_isSealed) {
                throw new InvalidOperationException("The plugin registry is sealed and cannot be changed anymore");
            }
        }

        /// <inheritdoc/>
        private IPluginRegistration RegisterCore(Type serviceType, IPlugin plugin)
        {
            EnsureNotSealed();
            var acceptRegistration = plugin.OnRegistration(this);

            int pluginId;

            if (acceptRegistration) {
                pluginId = _pluginRegistrationCounter++;
            } else {
                pluginId = -1;
            }

            var pluginRegistration = new PluginRegistration(pluginId, serviceType, plugin);

            if (acceptRegistration) {
                _pluginRegistrations.Add(pluginRegistration);
                NotifyPluginRegistrationConsumers(pluginRegistration);
            }

            return pluginRegistration;
        }

        /// <inheritdoc/>
        public IPluginRegistration Register(Type serviceType, IPlugin plugin)
        {
            var pluginType = plugin.GetType();

            if (!serviceType.IsAssignableFrom(plugin.GetType())) {
                throw new ArgumentException($"Service type \"{serviceType}\" is not a subset of plugin type \"{pluginType.GetType()}\"", nameof(serviceType));
            }

            return RegisterCore(serviceType, plugin);
        }

        /// <summary>
        /// Seals the registry.
        /// </summary>
        public void Seal() =>
            _isSealed = true;

        /// <inheritdoc/>
        public ILazyPlugin<T> First<T>()
            where T : IPlugin =>
            new LazyPlugin<T>(this);

        /// <inheritdoc/>
        public void Dispose() =>
            _consumePluginRegistrationActionList.Clear();

        private class Disposable : IDisposable
        {
            Action _action;

            public Disposable(Action action) => _action = action;

            public void Dispose() => _action();
        }
    }
}
