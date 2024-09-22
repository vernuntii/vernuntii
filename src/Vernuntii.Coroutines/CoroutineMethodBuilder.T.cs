using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

public partial struct CoroutineMethodBuilder<TResult>
{
    public static CoroutineMethodBuilder<TResult> Create()
    {
        return new CoroutineMethodBuilder<TResult>();
    }

    public Coroutine<TResult> Task {
        get {
            var stateMachineHolder = _stateMachineHolder ??= CreateWeaklyTyedStateMachineBox();
            return new Coroutine<TResult>(new ValueTask<TResult>(stateMachineHolder, stateMachineHolder.Version), stateMachineHolder);
        }
    }

    private CoroutineStateMachineHolder<TResult> _stateMachineHolder;

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => 
        _ = GetStateMachineHolder(ref stateMachine, ref _stateMachineHolder);

    public readonly void SetException(Exception e) => _stateMachineHolder.SetException(e);

    public readonly void SetResult(TResult result) => _stateMachineHolder.SetResult(result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        AwaitOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineHolder);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineHolder);

    internal IAsyncIteratorStateMachineHolder<TResult> ReplaceAsyncIteratorUnderlyingStateMachine<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine =>
        RenewAsyncIteratorStateMachineHolder(ref stateMachine, ref _stateMachineHolder);

    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new NotImplementedException();
}
