namespace Vernuntii.Reactive;

internal class DelegatingDisposable<T> : IDelegatingDisposable
{
    public bool IsDisposed =>
        _disposeHandler == null;

    internal Action<T>? _disposeHandler;
    internal T _state;

    private protected DelegatingDisposable() =>
        _state = default!;

    public DelegatingDisposable(Action<T> disposeHandler, T state)
    {
        _disposeHandler = disposeHandler ?? throw new ArgumentNullException(nameof(disposeHandler));
        _state = state;
    }

    internal DelegatingDisposable(Func<IDisposableLifetime, (Action<T> DisposeHandler, T State)> argumentsFactory) =>
        (_disposeHandler, _state) = argumentsFactory(this);

    public void Dispose()
    {
        var disposeHandler = _disposeHandler;

        if (Interlocked.CompareExchange(ref _disposeHandler, null, disposeHandler) == null) {
            // Too slow or already disposed
            return;
        }

        disposeHandler!(_state);
    }
}
