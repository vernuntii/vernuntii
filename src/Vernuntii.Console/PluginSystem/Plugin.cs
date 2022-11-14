using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// A plugin for <see cref="Vernuntii"/>.
    /// </summary>
    public abstract class Plugin : IPlugin, IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// If <see langword="true"/> the plugin is disposed.
        /// </summary>
        public bool IsDisposed => _isDisposed != 0;

        private int _isDisposed;

        /// <summary>
        /// Represents the plugin event aggregator.
        /// </summary>
        protected internal IPluginEventCache Events =>
            _eventAggregator ?? throw new InvalidOperationException($"Method {nameof(OnExecution)} was not called yet");

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
            if (IsDisposed) {
                disposable.Dispose();
            } else {
                _disposables.Add(disposable);
            }

            return disposable;
        }

        /// <summary>
        /// Called when this plugin gets notified about event aggregator.
        /// </summary>
        protected virtual void OnExecution()
        {
        }

        /// <summary>
        /// Called when this plugin gets notified about event aggregator.
        /// </summary>
        protected virtual ValueTask OnExecutionAsync() =>
            ValueTask.CompletedTask;

        ValueTask IPlugin.OnExecution(IPluginEventCache eventAggregator)
        {
            _eventAggregator = eventAggregator;
            OnExecution();
            return OnExecutionAsync();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">True disposes managed state (managed objects).</param>
        protected virtual void Dispose(bool disposing)
        {

        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">True disposes managed state (managed objects).</param>
        protected virtual void DisposeCore(bool disposing)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) {
                return;
            }

            if (!disposing) {
                return;
            }

            foreach (var disposable in _disposables) {
                disposable.Dispose();
            }

            Dispose(disposing);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            Dispose(disposing: false);
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
            return ValueTask.CompletedTask;
        }
    }
}
