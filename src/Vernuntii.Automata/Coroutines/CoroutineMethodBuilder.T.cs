using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public partial struct CoroutineMethodBuilder<T>
{
    public static CoroutineMethodBuilder<T> Create()
    {
        return new CoroutineMethodBuilder<T>();
    }

    public unsafe Coroutine<T> Task {
        get {
            fixed (CoroutineMethodBuilder<T>* builder = &this) {
                var stateMachineBox = _stateMachineBox ??= CreateWeaklyTyedStateMachineBox();
                _coroutineNode.SetResultStateMachine(stateMachineBox);
                return new Coroutine<T>(new ValueTask<T>(stateMachineBox, stateMachineBox.Version), builder);
            }
        }
    }

    private CoroutineStackNode _coroutineNode;
    private CoroutineStateMachineBox _stateMachineBox;

    internal void SetCoroutineNode(ref CoroutineStackNode parentNode)
    {
        parentNode.InitializeChildCoroutine(ref _coroutineNode);
    }

    public unsafe void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        _ = GetStateMachineBox(ref stateMachine, ref _stateMachineBox);
    }

    internal unsafe void Start()
    {
        _coroutineNode.Start();
        Unsafe.As<ICoroutineStateMachineBox>(_stateMachineBox).MoveNext();
    }

    public void SetException(Exception e)
    {
        _stateMachineBox.SetException(e);
        _coroutineNode.Stop();
    }

    public void SetResult(T result)
    {
        _stateMachineBox.SetResult(result);
        _coroutineNode.Stop();
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
        CoroutineMethodBuilderCore.AttemptHandlingCoroutineAwaiter(ref awaiter, ref _coroutineNode);
        AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new NotImplementedException();
}
