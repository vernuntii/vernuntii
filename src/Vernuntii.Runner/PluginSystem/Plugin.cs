using System.Diagnostics.CodeAnalysis;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// A plugin for <see cref="Vernuntii"/>.
    /// </summary>
    public abstract class Plugin : IPlugin, IDisposableRegistrar, IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// If <see langword="true"/> the plugin is disposed.
        /// </summary>
        [MemberNotNullWhen(false, nameof(_disposables))]
        public bool IsDisposed => _isDisposed != 0;

        private int _isDisposed;

        /// <summary>
        /// Represents the plugin event system.
        /// </summary>
        /// <remarks>
        /// You can set <see cref="PluginEventSystem.AutoUnsubscribeEvents"/>. It is <see langword="true"/> by default.
        /// <inheritdoc cref="PluginEventSystem.AutoUnsubscribeEvents" path="/summary"/>
        /// </remarks>
        protected internal PluginEventSystem Events =>
            _events ?? throw new InvalidOperationException($"Method {nameof(OnExecution)} was not called yet");

        private readonly PluginEventSystem _events;

        private IList<object>? _disposables = new List<object>();

        protected Plugin() =>
            _events = new(this) { AutoUnsubscribeEvents = true };

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

        void IDisposableRegistrar.AddDisposable(IDisposable disposable) =>
            AddDisposable(disposable);

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
        /// Called when this plugin got notified about the event system.
        /// The plugin base method implementation of this must not be called.
        /// </summary>
        protected virtual void OnExecution()
        {
        }

        /// <summary>
        /// Called when this plugin got notified about the event system.
        /// This base method does not have to be called if the next base type is <see cref="Plugin"/>.
        /// </summary>
        protected virtual Task OnExecutionAsync() =>
            Task.CompletedTask;

        Task IPlugin.OnExecution(IEventSystem eventSystem)
        {
            _events.InitializeEventSystem(eventSystem);
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
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
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
            await DisposePluginDisposablesAsync(synchronously: false).ConfigureAwait(false);
            DisposeOnce(disposing: false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The event system owned by one plugin.
        /// </summary>
        protected internal class PluginEventSystem : IEventSystem
        {
            /// <summary>
            /// If <see langword="true"/>, then the created event chains from <see cref="Events"/> will inherit the disposable registrar of this plugin.
            /// The event that build on an event chain inherits the registrar from said event chain.
            /// </summary>
            public bool AutoUnsubscribeEvents {
                get => _usingUnsubscriptionRegistrar is not null;

                set {
                    if (value) {
                        _usingUnsubscriptionRegistrar = _onRequestUsingUnsubscriptionRegistrar;
                    } else {
                        _usingUnsubscriptionRegistrar = null;
                    }
                }
            }

            /// <summary>
            /// The created event chain from this factory will inherit this registrar.
            /// The event that build on an event chain inherits the registrar from said event chain.
            /// </summary>
            private IDisposableRegistrar? _usingUnsubscriptionRegistrar;

            private readonly IDisposableRegistrar _onRequestUsingUnsubscriptionRegistrar;
            private IEventSystem? _eventSystem;

            internal PluginEventSystem(IDisposableRegistrar onRequestUsingUnsubscriptionRegistrar) =>
                _onRequestUsingUnsubscriptionRegistrar = onRequestUsingUnsubscriptionRegistrar;

            internal void InitializeEventSystem(IEventSystem eventSystem)
            {
                if (_eventSystem is not null) {
                    throw new InvalidOperationException("Event system is already initialized");
                }

                _eventSystem = eventSystem ?? throw new ArgumentNullException(nameof(eventSystem));
            }

            [MemberNotNull(nameof(_eventSystem))]
            private void ThrowIfEventSystemIsUninitialized()
            {
                if (_eventSystem is null) {
                    throw new InvalidOperationException($"Method {nameof(OnExecution)} was not called yet");
                }
            }

            Task IDistinguishableEventFulfiller.FullfillAsync<T>(object eventId, T eventData)
            {
                ThrowIfEventSystemIsUninitialized();
                return _eventSystem.FullfillAsync(eventId, eventData);
            }

            EventChain<T> IEventChainFactory.Create<T>(EventChainFragment<T> fragment)
            {
                ThrowIfEventSystemIsUninitialized();

                return _eventSystem.Create(fragment) with {
                    UnsubscriptionRegistrar = _usingUnsubscriptionRegistrar
                };
            }
        }
    }
}
