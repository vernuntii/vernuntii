using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal struct CoroutineStackNode : ICoroutineHandler
{
    private int _identifier;
    private CoroutineContext _context;
    private ICoroutineResultStateMachine? _resultStateMachine;

    internal ICoroutineResultStateMachine ResultStateMachine {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            return _resultStateMachine ?? throw new InvalidOperationException("Coroutine node has not been initialized yet");
        }
    }

    public CoroutineStackNode(CoroutineContext context)
    {
        _context = context;
    }

    internal void InitializeChildCoroutine(ref CoroutineStackNode childNode)
    {
        childNode._context = _context;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Start()
    {
        _identifier = _context.AddCoroutineNode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetResultStateMachine(ICoroutineResultStateMachine resultStateMachine)
    {
        _resultStateMachine = resultStateMachine;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICoroutineHandler.HandleChildCoroutine(ref Coroutine.CoroutineAwaiter coroutineAwaiter)
    {
        coroutineAwaiter.PropagateCoroutineNode(ref this);
        coroutineAwaiter.StartStateMachine();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICoroutineHandler.HandleChildCoroutine(ref Coroutine<object>.CoroutineAwaiter coroutineAwaiter)
    {
        coroutineAwaiter.PropagateCoroutineNode(ref this);
        coroutineAwaiter.StartStateMachine();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    unsafe void ICoroutineHandler.HandleDirectCoroutine([NotNull] CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        var argumentReceiver = new CoroutineArgumentReceiver(ref this);
        argumentReceiverDelegate(ref argumentReceiver);
    }

    public void Stop()
    {
        _context.RemoveCoroutineNode();
        _context = null!;
        _resultStateMachine = null;
    }
}
