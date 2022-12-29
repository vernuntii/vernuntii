namespace Vernuntii.Reactive;

internal class DelegatingDisposable<T> : IDisposable
{
    private Action<T>? _disposeHandler;
    private readonly T _state;

    public DelegatingDisposable(Action<T> disposeHandler, T state)
    {
        _disposeHandler = disposeHandler ?? throw new ArgumentNullException(nameof(disposeHandler));
        _state = state;
    }

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
