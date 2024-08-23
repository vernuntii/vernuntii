using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal struct CoroutineContext : ICoroutineHandler
{
    internal CoroutineScope _scope;

    private int _identifier;
    private ICoroutineResultStateMachine? _resultStateMachine;

    internal ICoroutineResultStateMachine ResultStateMachine {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            Debug.Assert(_resultStateMachine != null);
            return _resultStateMachine ?? throw new InvalidOperationException("Coroutine node has not been initialized yet");
        }
    }

    public CoroutineContext(CoroutineScope scope)
    {
        _scope = scope;
        _resultStateMachine = CoroutineMethodBuilder<VoidCoroutineResult>.CoroutineStateMachineBox.m_syncSuccessSentinel;
    }

    internal readonly void BequestContext(ref CoroutineContext childContext)
    {
        childContext._scope = _scope;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void OnCoroutineStarted()
    {
        _identifier = _scope.AddCoroutineContext();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetResultStateMachine(ICoroutineResultStateMachine resultStateMachine)
    {
        _resultStateMachine = resultStateMachine;
    }

    void ICoroutineHandler.HandleChildCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutineAwaiter)
    {
        coroutineAwaiter.InheritCoroutineContext(ref this);
        coroutineAwaiter.StartCoroutine();
    }

    void ICoroutineHandler.HandleSiblingCoroutine<TCoroutine>(ref TCoroutine coroutine)
    {
        var argumentReceiver = new CoroutineArgumentReceiver(ref this);
        coroutine.AcceptCoroutineArgumentReceiver(ref argumentReceiver);
    }

    public void OnCoroutineCompleted()
    {
        _scope.RemoveCoroutineContext();
        _scope = null!;
        _resultStateMachine = null;
    }
}
