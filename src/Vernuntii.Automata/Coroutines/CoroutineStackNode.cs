using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal struct CoroutineStackNode : ICoroutineHandler
{
    private int _identifier;
    internal CoroutineContext Context;
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
        Context = context;
        _resultStateMachine = CoroutineMethodBuilder<VoidCoroutineResult>.CoroutineStateMachineBox.m_syncSuccessSentinel;
    }

    internal void InitializeChildCoroutine(ref CoroutineStackNode childNode)
    {
        childNode.Context = Context;
        //if (childNode._resultStateMachine is null) {
        //    childNode._resultStateMachine = ;
        //}
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Start()
    {
        _identifier = Context.AddCoroutineNode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetResultStateMachine(ICoroutineResultStateMachine resultStateMachine)
    {
        _resultStateMachine = resultStateMachine;
    }

    void ICoroutineHandler.HandleChildCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutineAwaiter)
    {
        coroutineAwaiter.InheritCoroutineNode(ref this);
        coroutineAwaiter.StartCoroutine();
    }

    void ICoroutineHandler.HandleSiblingCoroutine<TCoroutine>(ref TCoroutine coroutine)
    {
        var argumentReceiver = new CoroutineArgumentReceiver(ref this);
        coroutine.AcceptCoroutineArgumentReceiver(ref argumentReceiver);
    }

    public void Stop()
    {
        Context.RemoveCoroutineNode();
        Context = null!;
        _resultStateMachine = null;
    }
}
