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
        protected internal IPluginRegistry PluginRegistry =>
            _pluginRegistry ?? throw new InvalidOperationException($"Method {nameof(OnRegistration)} was not called yet");

        /// <summary>
        /// Represents the plugin event aggregator.
        /// </summary>
        protected internal IPluginEventAggregator EventAggregator =>
            _eventAggregator ?? throw new InvalidOperationException($"Method {nameof(OnSetEventAggregator)} was not called yet");

        private IPluginRegistry? _pluginRegistry;
        private IPluginEventAggregator? _eventAggregator;
        private List<IDisposable> _disposables = new List<IDisposable>();

        internal void AddDisposable(IDisposable disposable) =>
            _disposables.Add(disposable);

        /// <summary>
        /// Called when this plugin gets added. It gives
        /// you the opportunity to prepare dependencies.
        /// </summary>
        protected virtual void OnRegistration()
        {
        }

        void IPlugin.OnRegistration(IPluginRegistry pluginRegistry)
        {
            _pluginRegistry = pluginRegistry;
            OnRegistration();
        }

        /// <summary>
        /// Called when all plugins are registered and ordered.
        /// </summary>
        protected virtual void OnCompletedRegistration()
        {
        }

        void IPlugin.OnCompletedRegistration() =>
            OnCompletedRegistration();

        /// <summary>
        /// Called when this plugin gets notified about event aggregator.
        /// Called after <see cref="OnRegistration()"/>.
        /// </summary>
        protected virtual void OnSetEventAggregator()
        {
        }

        void IPlugin.OnEventAggregation(IPluginEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            OnSetEventAggregator();
        }

        /// <summary>
        /// Subscribes to event that gets canceled when the plugin is disposed.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="action"></param>
        protected void SubscribeEvent<TEvent>(Action action)
            where TEvent : PubSubEvent, new() =>
            AddDisposable(EventAggregator.GetEvent<TEvent>().Subscribe(action));

        /// <summary>
        /// Subscribes to event that gets canceled when the plugin is disposed.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="action"></param>
        protected void SubscribeEvent<TEvent, TPayload>(Action<TPayload> action)
            where TEvent : PubSubEvent<TPayload>, new() =>
            AddDisposable(EventAggregator.GetEvent<TEvent>().Subscribe(action));

        /// <summary>
        /// Subscribes to event that gets canceled when the plugin is disposed.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="discriminator"></param>
        /// <param name="action"></param>
        /// <returns>The actual event you can subscribe to.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        protected void SubscribeEvent<TEvent, TPayload>(PubSubEvent<TEvent, TPayload> discriminator, Action<TPayload> action)
            where TEvent : PubSubEvent<TEvent, TPayload>, new() =>
            AddDisposable(EventAggregator.GetEvent<TEvent>().Subscribe(action));

        /// <summary>
        /// Called when plugin gets explictly destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
        }

        void IPlugin.OnDestroy() => OnDestroy();

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
