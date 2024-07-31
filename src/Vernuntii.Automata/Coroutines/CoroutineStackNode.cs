using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal struct CoroutineStackNode
{
    private int _depth;
    private CoroutineContext _context;

    public CoroutineStackNode(CoroutineContext context)
    {
        _context = context;
    }

    internal void InitializeChildCoroutine(ref CoroutineStackNode childNode)
    {
        childNode._depth = _context.NodesCount;
        childNode._context = _context;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Start()
    {
        
    }

    internal void HandleInlineCoroutine([NotNull] CoroutineArgumentReceiverAcceptor argumentReceiverAcceptor)
    {
        var argumentReceiver = new CoroutineArgumentReceiver();
        argumentReceiverAcceptor.Invoke(argumentReceiver);
    }

    public void Stop()
    {
        //if (Interlocked.Decrement(ref _depth) <= 0) {
        //    Dispose(true);
        //}
    }

    //private bool _disposedValue;

    //private void Dispose(bool disposing)
    //{
    //    if (disposing) {

    //    }
    //}

    //void IDisposable.Dispose()
    //{
    //    Dispose(true);
    //    GC.SuppressFinalize(this);
    //}
}
