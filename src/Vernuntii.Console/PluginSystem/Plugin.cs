using System.Diagnostics.CodeAnalysis;
using Vernuntii.PluginSystem.Reactive;

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
        [MemberNotNullWhen(false, nameof(_disposables))]
        public bool IsDisposed => _isDisposed != 0;

        private int _isDisposed;

        /// <summary>
        /// Represents the plugin event aggregator.
        /// </summary>
        protected internal EventSystem Events =>
            _eventSystem ?? throw new InvalidOperationException($"Method {nameof(OnExecution)} was not called yet");

        private EventSystem? _eventSystem;
        private IList<object>? _disposables = new List<object>();

        [MemberNotNull(nameof(_disposables))]
        private void ThrowIfDisposed()
        {
            if (IsDisposed) {
                throw new ObjectDisposedException("The object has been disposed");
            }
        }

        /// <summary>
        /// Adds a disposable that gets disposed when the plugin gets disposed.
        /// </summary>
        /// <param name="disposable"></param>
        /// <exception cref="ObjectDisposedException"/>
        protected internal void AddDisposable(IDisposable disposable)
        {
            ThrowIfDisposed();
            _disposables.Add(disposable);
        }

        /// <summary>
        /// Adds a disposable that gets disposed when the plugin gets disposed.
        /// </summary>
        /// <param name="disposable"></param>
        /// <exception cref="ObjectDisposedException"/>
        protected internal void AddDisposable(IAsyncDisposable disposable)
        {
            ThrowIfDisposed();
            _disposables.Add(disposable);
        }

        /// <summary>
        /// Adds a disposable that gets disposed when the plugin gets disposed.
        /// </summary>
        /// <param name="disposable"></param>
        /// <exception cref="ObjectDisposedException"/>
        protected internal void AddDisposable<T>(T disposable)
            where T : IDisposable, IAsyncDisposable
        {
            ThrowIfDisposed();
            _disposables.Add(disposable);
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

        ValueTask IPlugin.OnExecution(EventSystem eventSystem)
        {
            _eventSystem = eventSystem;
            OnExecution();
            return OnExecutionAsync();
        }

        /// <inheritdoc cref="DisposeAsync"/>
        protected virtual ValueTask DisposeAsyncCore() =>
            ValueTask.CompletedTask;

        /// <inheritdoc cref="IDisposable.Dispose"/>
        /// <param name="disposing">True disposes managed state (managed objects).</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        private async ValueTask DisposePluginDisposablesAsync(bool synchronously)
        {
            var disposables = _disposables;

            if (Interlocked.CompareExchange(ref _disposables, null, disposables) != null) {
                foreach (var untypedDisposable in disposables!) {
                    // Prefer asynchronous dispose over synchronous dispose
                    if (!synchronously && untypedDisposable is IAsyncDisposable asyncDisposable) {
                        await asyncDisposable.DisposeAsync().ConfigureAwait(true);
                        continue;
                    }

                    // If asynchronous and not async-disposable then fallback to synchronous dispose
                    if (untypedDisposable is IDisposable disposable) {
                        disposable.Dispose();
                    }
                }
            }
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        /// <param name="disposing">True disposes managed state (managed objects).</param>
        private void DisposeOnce(bool disposing)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) {
                return;
            }

            Dispose(disposing);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            DisposePluginDisposablesAsync(synchronously: true).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            DisposeOnce(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            await DisposePluginDisposablesAsync(synchronously: false).ConfigureAwait(true);
            DisposeOnce(disposing: false);
            GC.SuppressFinalize(this);
        }
    }
}
