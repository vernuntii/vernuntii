namespace Vernuntii.Coroutines;

public struct CoroutineMethodBuilder
{
    public static CoroutineMethodBuilder Create()
    {
        return new CoroutineMethodBuilder();
    }

    public Coroutine Task {
        get {
            var stateMachineBox = _stateMachineBox ??= CoroutineMethodBuilder<Nothing>.CreateWeaklyTyedStateMachineBox();
            return new Coroutine(new ValueTask(stateMachineBox, stateMachineBox.Version), stateMachineBox);
        }
    }

    private CoroutineMethodBuilder<Nothing>.CoroutineStateMachineBox _stateMachineBox;

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => 
        _ = CoroutineMethodBuilder<Nothing>.GetStateMachineBox(ref stateMachine, ref _stateMachineBox);

    public readonly void SetException(Exception e) => _stateMachineBox.SetException(e);

    public readonly void SetResult() => _stateMachineBox.SetResult(default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        CoroutineMethodBuilder<Nothing>.AwaitOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        CoroutineMethodBuilder<Nothing>.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);

    internal ICoroutineStateMachineBox ReplaceStateMachine<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine =>
        CoroutineMethodBuilder<Nothing>.RenewStateMachineBox(ref stateMachine, ref _stateMachineBox);

    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new NotImplementedException();
}
