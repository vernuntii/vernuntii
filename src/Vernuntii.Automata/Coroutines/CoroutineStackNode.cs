using System.Diagnostics;
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
            Debug.Assert(_resultStateMachine != null);
            return _resultStateMachine ?? throw new InvalidOperationException("Coroutine node has not been initialized yet");
        }
    }

    public CoroutineStackNode(CoroutineContext context)
    {
        _context = context;
        _resultStateMachine = CoroutineMethodBuilder<VoidCoroutineResult>.CoroutineStateMachineBox.m_syncSuccessSentinel;
    }

    internal void InitializeChildCoroutine(ref CoroutineStackNode childNode)
    {
        childNode._context = _context;
        //if (childNode._resultStateMachine is null) {
        //    childNode._resultStateMachine = ;
        //}
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

    void ICoroutineHandler.HandleChildCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutineAwaiter)
    {
        coroutineAwaiter.PropagateCoroutineNode(ref this);
        coroutineAwaiter.StartStateMachine();
    }

    void ICoroutineHandler.HandleSiblingCoroutine<TCoroutine>(ref TCoroutine coroutine)
    {
        var argumentReceiver = new CoroutineArgumentReceiver(ref this);
        coroutine.AcceptCoroutineArgumentReceiver(ref argumentReceiver);
    }

    public void Stop()
    {
        _context.RemoveCoroutineNode();
        _context = null!;
        _resultStateMachine = null;
    }
}
