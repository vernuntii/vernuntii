using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal sealed class CoroutineContext : IDisposable
{
    int _depth;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void OnStartingCoroutine(ref CoroutineContext coroutineContext)
    {
        Interlocked.Increment(ref _depth);
    }

    internal void HandleCoroutineInvocation([NotNull] CoroutineArgumentReceiverAcceptor argumentReceiverAcceptor)
    {
        var argumentReceiver = new CoroutineArgumentReceiver();
        argumentReceiverAcceptor.Invoke(argumentReceiver);
    }

    private bool _disposedValue;

    private void Dispose(bool disposing)
    {
        if (disposing) {

        }
    }

    public void OnStoppingCoroutine()
    {
        if (Interlocked.Decrement(ref _depth) <= 0) {
            Dispose(true);
        }
    }

    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
