using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public struct CoroutineMethodBuilder
{
    public static CoroutineMethodBuilder Create()
    {
        return new CoroutineMethodBuilder();
    }

    public Coroutine Task {
        get {
            var stateMachineBox = _stateMachineBox ??= CoroutineMethodBuilder<VoidResult>.CreateWeaklyTyedStateMachineBox();
            return new Coroutine(new ValueTask(stateMachineBox, stateMachineBox.Version), stateMachineBox);
        }
    }

    private CoroutineMethodBuilder<VoidResult>.CoroutineStateMachineBox _stateMachineBox;

    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        _ = CoroutineMethodBuilder<VoidResult>.GetStateMachineBox(ref stateMachine, ref _stateMachineBox);
    }

    public void SetException(Exception e)
    {
        _stateMachineBox.SetException(e);
    }

    public void SetResult()
    {
        _stateMachineBox.SetResult(default);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        CoroutineMethodBuilder<VoidResult>.AwaitOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        CoroutineMethodBuilder<VoidResult>.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);

    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new NotImplementedException();
}
