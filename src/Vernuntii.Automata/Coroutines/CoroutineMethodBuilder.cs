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
            var stateMachineBox = _stateMachineBox ??= CoroutineMethodBuilder<VoidCoroutineResult>.CreateWeaklyTyedStateMachineBox();
            return new Coroutine(new ValueTask(stateMachineBox, stateMachineBox.Version), stateMachineBox);
        }
    }

    private CoroutineMethodBuilder<VoidCoroutineResult>.CoroutineStateMachineBox _stateMachineBox;

    // Gets called prior access of Task in non-debugging cases.
    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        _ = CoroutineMethodBuilder<VoidCoroutineResult>.GetStateMachineBox(ref stateMachine, ref _stateMachineBox);
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
        CoroutineMethodBuilder<VoidCoroutineResult>.AwaitOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        CoroutineMethodBuilderCore.AttemptHandlingCoroutineAwaiter(ref awaiter, ref _stateMachineBox.CoroutineNode);
        CoroutineMethodBuilder<VoidCoroutineResult>.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new NotImplementedException();
}
