using System.Collections;
using Microsoft.Collections.Extensions;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// The plugin registry.
    /// </summary>
    public sealed class PluginRegistry : IPluginRegistry, IDisposable
    {
        /// <inheritdoc/>
        public IReadOnlyList<IPluginRegistration> PluginRegistrations => _pluginRegistrations.Values;

        private List<Action<IPluginRegistration>> _consumePluginRegistrationActionList =
            new List<Action<IPluginRegistration>>();

        private OrderedDictionary<int, PluginRegistration> _pluginRegistrations = new OrderedDictionary<int, PluginRegistration>();

        private int _pluginRegistrationCounter;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public PluginRegistry()
        {
        }

        private void NotifyPluginRegistrationConsumer(Action<IPluginRegistration> consumePluginRegistrationAction)
        {
            foreach (var pluginRegistration in _pluginRegistrations.Values) {
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

        /// <inheritdoc/>
        private IPluginRegistration RegisterCore(Type serviceType, IPlugin plugin)
        {
            plugin.OnRegistration(this);
            var pluginRegistration = new PluginRegistration(_pluginRegistrationCounter++, serviceType, plugin);
            _pluginRegistrations.Add(pluginRegistration.PluginId, pluginRegistration);
            NotifyPluginRegistrationConsumers(pluginRegistration);
            return pluginRegistration;
        }

        /// <inheritdoc/>
        public IPluginRegistration Register(Type serviceType, IPlugin plugin)
        {
            var pluginType = plugin.GetType();

            if (!serviceType.IsAssignableFrom(plugin.GetType())) {
                throw new ArgumentException($"Service type \"{serviceType}\" is not a subset of type \"{pluginType.GetType()}\"", nameof(serviceType));
            }

            return RegisterCore(serviceType, plugin);
        }

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
