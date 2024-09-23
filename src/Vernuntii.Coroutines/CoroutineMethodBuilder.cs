using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

public struct CoroutineMethodBuilder
{
    public static CoroutineMethodBuilder Create()
    {
        return new CoroutineMethodBuilder();
    }

    public Coroutine Task {
        get {
            var stateMachineHolder = _stateMachineHolder ??= CoroutineMethodBuilder<Nothing>.CreateWeaklyTyedStateMachineBox();
            return new Coroutine(new ValueTask(stateMachineHolder, stateMachineHolder.Version), stateMachineHolder);
        }
    }

    private CoroutineStateMachineHolder<Nothing> _stateMachineHolder;

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
        _ = CoroutineMethodBuilder<Nothing>.GetStateMachineHolder(ref stateMachine, ref _stateMachineHolder);

    public readonly void SetException(Exception e) => _stateMachineHolder.SetException(e);

    public readonly void SetResult() => _stateMachineHolder.SetResult(default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        CoroutineMethodBuilder<Nothing>.AwaitOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineHolder);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        CoroutineMethodBuilder<Nothing>.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineHolder);

    internal IAsyncIteratorStateMachineHolder<Nothing> ReplaceCoroutineUnderlyingStateMachine<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine =>
        CoroutineMethodBuilder<Nothing>.RenewCoroutineStateMachineHolder(ref stateMachine, ref _stateMachineHolder);

    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new NotImplementedException();
}
