using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public struct AsyncCoroutineMethodBuilder<T>
{
    public static AsyncCoroutineMethodBuilder<T> Create()
    {
        return new AsyncCoroutineMethodBuilder<T>();
    }

    public unsafe Coroutine<T> Task {
        get {
            fixed (AsyncCoroutineMethodBuilder<T>* builder = &this) {
                return new Coroutine<T>(_builder.Task, builder);
            }
        }
    }

    private PoolingAsyncValueTaskMethodBuilder<T> _builder; // Must not be readonly due to mutable struct
    internal unsafe Action? _stateMachineInitiator;
    private CoroutineStackNode _coroutineNode;

    [DebuggerStepThrough]
    public unsafe void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        _stateMachineInitiator = stateMachine.MoveNext;
    }

    internal void SetCoroutineNode(ref CoroutineStackNode parentNode)
    {
        parentNode.InitializeChildCoroutine(ref _coroutineNode);
    }

    internal unsafe void Start()
    {
        _coroutineNode.Start();
        _stateMachineInitiator?.Invoke();
        _stateMachineInitiator = null;
    }

    public void SetException(Exception e)
    {
        _coroutineNode.Stop();
        _builder.SetException(e);
    }

    public void SetResult(T result)
    {
        _coroutineNode.Stop();
        _builder.SetResult(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        _builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        AsyncCoroutineMethodBuilderCore.ProcessAwaiterBeforeAwaitingOnCompleted(ref awaiter, ref _coroutineNode);
        _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        _builder.SetStateMachine(stateMachine);
    }
}
