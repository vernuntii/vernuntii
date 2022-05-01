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

        /// <summary>
        /// Represents the plugin registry.
        /// </summary>
        protected internal IPluginRegistry Plugins =>
            _plugins ?? throw new InvalidOperationException($"Method {nameof(OnRegistrationAsync)} was not called yet");

        /// <summary>
        /// Represents the plugin event aggregator.
        /// </summary>
        protected internal IPluginEventCache Events =>
            _eventAggregator ?? throw new InvalidOperationException($"Method {nameof(OnEvents)} was not called yet");

        private IPluginRegistry? _plugins;
        private IPluginEventCache? _eventAggregator;
        private List<IDisposable> _disposables = new List<IDisposable>();

        internal void AddDisposable(IDisposable disposable) =>
            _disposables.Add(disposable);

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
            var registrationContext = new RegistrationContext();
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

        /// <summary>
        /// Gets the first plugin of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="InvalidOperationException"></exception>
        protected T FirstPlugin<T>()
            where T : IPlugin =>
            Plugins.First<T>().Value;

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

        ///// <summary>
        ///// Subscribes to event that gets canceled when the plugin is disposed.
        ///// </summary>
        ///// <typeparam name="TEvent"></typeparam>
        ///// <param name="action"></param>
        //protected void SubscribeEvent<TEvent>(Action action)
        //    where TEvent : PubSubEvent, new() =>
        //    AddDisposable(Events.GetEvent<TEvent>().Subscribe(action));

        ///// <summary>
        ///// Subscribes to event that gets canceled when the plugin is disposed.
        ///// </summary>
        ///// <typeparam name="TEvent"></typeparam>
        ///// <typeparam name="TPayload"></typeparam>
        ///// <param name="action"></param>
        //protected void SubscribeEvent<TEvent, TPayload>(Action<TPayload> action)
        //    where TEvent : PubSubEvent<TPayload>, new() =>
        //    AddDisposable(Events.GetEvent<TEvent>().Subscribe(action));

        ///// <summary>
        ///// Subscribes to event that gets canceled when the plugin is disposed.
        ///// </summary>
        ///// <typeparam name="TEvent"></typeparam>
        ///// <typeparam name="TPayload"></typeparam>
        ///// <param name="discriminator"></param>
        ///// <param name="action"></param>
        ///// <returns>The actual event you can subscribe to.</returns>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Justification = "<Pending>")]
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        //protected void SubscribeEvent<TEvent, TPayload>(PubSubEvent<TEvent, TPayload> discriminator, Action<TPayload> action)
        //    where TEvent : PubSubEvent<TEvent, TPayload>, new() =>
        //    AddDisposable(Events.GetEvent<TEvent>().Subscribe(action));

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
            /// <see langword="true"/> accepts registration,
            /// <see langword="false"/> prevents registration.
            /// Default is <see langword="true"/>.
            /// </summary>
            public bool AcceptRegistration { get; set; } = true;
        }

        ///// <summary>
        ///// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        ///// </summary>
        ///// <returns>A task that represents the asynchronous dispose operation.</returns>
        //protected virtual ValueTask DisposeAsyncCore() =>
        //    ValueTask.CompletedTask;

        ///// <inheritdoc/>
        //public async ValueTask DisposeAsync()
        //{
        //    await DisposeAsyncCore();
        //    Dispose(disposing: false);
        //    GC.SuppressFinalize(this);
        //}
    }
}
