namespace Vernuntii.PluginSystem
{
    internal class LazyPlugin<TPlugin> : ILazyPlugin<TPlugin>
        where TPlugin : IPlugin
    {
        public event PluginRegistered<TPlugin>? Registered {
            add {
                if (value == null) {
                    return;
                }

                if (IsRegistered) {
                    InvokeRegistered(value);
                } else {
                    _registered += value;
                }
            }

            remove => _registered -= value;
        }

        private event PluginRegistered<TPlugin>? _registered;

        public TPlugin Value {
            get => _value ?? throw new InvalidOperationException($"Plugin of type {typeof(TPlugin)} was not or is not yet registered.");
            private set => _value = value;
        }

        public bool IsRegistered => _value != null;

        private IDisposable _disposableConsumer;
        private TPlugin? _value;
        private Type _pluginType = typeof(TPlugin);

        public LazyPlugin(IPluginRegistrationProducer registrationProducer) =>
            _disposableConsumer = registrationProducer.AddPluginRegistrationConsumer(ConsumePluginRegistration);

        private void InvokeRegistered(PluginRegistered<TPlugin> onRegistered) =>
            onRegistered?.Invoke(Value);

        private void ConsumePluginRegistration(IPluginRegistration registration)
        {
            if (registration.ServiceType == _pluginType) {
                Value = (TPlugin)registration.Plugin;
                _disposableConsumer?.Dispose();

                if (_registered != null) {
                    InvokeRegistered(_registered.Invoke);
                    _registered = null;
                }
            }
        }
    }
}
