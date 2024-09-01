using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public partial struct CoroutineMethodBuilder<TResult>
{
    public static CoroutineMethodBuilder<TResult> Create()
    {
        return new CoroutineMethodBuilder<TResult>();
    }

    public Coroutine<TResult> Task {
        get {
            var stateMachineBox = _stateMachineBox ??= CreateWeaklyTyedStateMachineBox();
            return new Coroutine<TResult>(new ValueTask<TResult>(stateMachineBox, stateMachineBox.Version), stateMachineBox);
        }
    }

    private CoroutineStateMachineBox _stateMachineBox;

    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine => _ = GetStateMachineBox(ref stateMachine, ref _stateMachineBox);

    public readonly void SetException(Exception e) => _stateMachineBox.SetException(e);

    public readonly void SetResult(TResult result) => _stateMachineBox.SetResult(result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        AwaitOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);

    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new NotImplementedException();
}
