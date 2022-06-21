using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// A plugin for <see cref="Vernuntii"/>.
    /// </summary>
    public abstract class Plugin : IPlugin
    {
        /// <inheritdoc/>
        public virtual int? Order { get; }

        private bool _isDisposed;

        /// <summary>
        /// Represents the plugin registry.
        /// </summary>
        protected internal IReadOnlyPlugins Plugins =>
            _plugins ?? throw new InvalidOperationException($"Method {nameof(OnRegistrationAsync)} was not called yet");

        /// <summary>
        /// Represents the plugin event aggregator.
        /// </summary>
        protected internal IPluginEventCache Events =>
            _eventAggregator ?? throw new InvalidOperationException($"Method {nameof(OnEvents)} was not called yet");

        private IPluginRegistry? _plugins;
        private IPluginEventCache? _eventAggregator;
        private List<IDisposable> _disposables = new List<IDisposable>();

        /// <summary>
        /// Adds a disposable that gets disposed when the plugin gets disposed.
        /// If the plugin has been already disposed <paramref name="disposable"/>
        /// gets immediatelly disposed.
        /// </summary>
        /// <param name="disposable"></param>
        protected internal T AddDisposable<T>(T disposable)
            where T : IDisposable
        {
            if (_isDisposed) {
                disposable.Dispose();
            } else {
                _disposables.Add(disposable);
            }

            return disposable;
        }

        /// <summary>
        /// Called when this plugin gets added. It gives
        /// you the opportunity to prepare dependencies
        /// and prevent the registration.
        /// </summary>
        /// <param name="registrationContext"></param>
        protected virtual ValueTask OnRegistrationAsync(RegistrationContext registrationContext) =>
            ValueTask.CompletedTask;

        async ValueTask<bool> IPlugin.OnRegistration(IPluginRegistry pluginRegistry)
        {
            _plugins = pluginRegistry;
            var registrationContext = new RegistrationContext(pluginRegistry);
            await OnRegistrationAsync(registrationContext);
            return registrationContext.AcceptRegistration;
        }

        /// <summary>
        /// Called when all plugins are registered and ordered.
        /// </summary>
        protected virtual void OnCompletedRegistration()
        {
        }

        /// <summary>
        /// Called when all plugins are registered and ordered.
        /// </summary>
        protected virtual ValueTask OnCompletedRegistrationAsync() =>
            ValueTask.CompletedTask;

        ValueTask IPlugin.OnCompletedRegistration()
        {
            OnCompletedRegistration();
            return OnCompletedRegistrationAsync();
        }

        /// <summary>
        /// Called when this plugin gets notified about event aggregator.
        /// Called after <see cref="OnRegistrationAsync(RegistrationContext)"/>.
        /// </summary>
        protected virtual void OnEvents()
        {
        }

        /// <summary>
        /// Called when this plugin gets notified about event aggregator.
        /// Called after <see cref="OnRegistrationAsync(RegistrationContext)"/>.
        /// </summary>
        protected virtual ValueTask OnEventsAsync() =>
            ValueTask.CompletedTask;

        ValueTask IPlugin.OnEvents(IPluginEventCache eventAggregator)
        {
            _eventAggregator = eventAggregator;
            OnEvents();
            return OnEventsAsync();
        }

        /// <summary>
        /// Called when plugin gets explictly destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
        }

        /// <summary>
        /// Called when plugin gets explictly destroyed.
        /// </summary>
        protected virtual ValueTask OnDestroyAsync() =>
            ValueTask.CompletedTask;

        ValueTask IPlugin.OnDestroy()
        {
            OnDestroy();
            return OnDestroyAsync();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">True disposes managed state (managed objects).</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) {
                return;
            }

            foreach (var disposable in _disposables) {
                disposable.Dispose();
            }

            _isDisposed = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Describes the context of on-going registration.
        /// </summary>
        public class RegistrationContext
        {
            /// <summary>
            /// The plugin registry.
            /// </summary>
            public IPluginRegistry PluginRegistry { get; }

            /// <summary>
            /// <see langword="true"/> accepts registration,
            /// <see langword="false"/> prevents registration.
            /// Default is <see langword="true"/>.
            /// </summary>
            public bool AcceptRegistration { get; set; } = true;

            /// <summary>
            /// Creates an instance of this type.
            /// </summary>
            /// <param name="pluginRegistry"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public RegistrationContext(IPluginRegistry pluginRegistry) =>
                PluginRegistry = pluginRegistry ?? throw new ArgumentNullException(nameof(pluginRegistry));
        }
    }
}
