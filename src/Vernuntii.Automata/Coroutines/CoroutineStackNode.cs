using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal struct CoroutineStackNode : ICoroutineHandler
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
        _context.AddCoroutineNode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICoroutineHandler.HandleChildCoroutine(ref Coroutine.CoroutineAwaiter coroutineAwaiter)
    {
        coroutineAwaiter.PropagateCoroutineNode(ref this);
        coroutineAwaiter.StartStateMachine();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe void ICoroutineHandler.HandleDirectCoroutine([NotNull] CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        var ttt2 = new CoroutineArgumentReceiver(ref this);
        argumentReceiverDelegate(ref ttt2);
    }

    public void Stop()
    {
        _context.RemoveCoroutineNode();
    }
}
