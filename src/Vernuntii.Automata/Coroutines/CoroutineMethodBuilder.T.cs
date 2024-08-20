using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public partial struct CoroutineMethodBuilder<T>
{
    public static CoroutineMethodBuilder<T> Create()
    {
        return new CoroutineMethodBuilder<T>();
    }

    public Coroutine<T> Task {
        get {
            var stateMachineBox = _stateMachineBox ??= CreateWeaklyTyedStateMachineBox();
            return new Coroutine<T>(new ValueTask<T>(stateMachineBox, stateMachineBox.Version), stateMachineBox);
        }
    }

    private CoroutineStateMachineBox _stateMachineBox;

    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        _ = GetStateMachineBox(ref stateMachine, ref _stateMachineBox);
    }

    public void SetException(Exception e)
    {
        _stateMachineBox.SetException(e);
    }

    public void SetResult(T result)
    {
        _stateMachineBox.SetResult(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        AwaitOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        CoroutineMethodBuilderCore.AttemptHandlingCoroutineAwaiter(ref awaiter, ref _stateMachineBox.CoroutineNode);
        AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new NotImplementedException();
}
