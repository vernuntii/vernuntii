using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public struct AsyncCoroutineMethodBuilder
{
    public static AsyncCoroutineMethodBuilder Create()
    {
        return new AsyncCoroutineMethodBuilder();
    }

    public unsafe Coroutine Task {
        get {
            fixed (AsyncCoroutineMethodBuilder* builder = &this) {
                return new Coroutine(_builder.Task, builder);
            }
        }
    }

    private PoolingAsyncValueTaskMethodBuilder _builder; // Must not be readonly due to mutable struct
    internal unsafe Action? _stateMachineInitiator;
    private CoroutineContext _coroutineContext;

    public unsafe void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        _stateMachineInitiator = stateMachine.MoveNext;
    }

    [DebuggerStepThrough]
    internal unsafe void Start()
    {
        _coroutineContext.OnStartingCoroutine(ref _coroutineContext);
        _stateMachineInitiator?.Invoke();
        _stateMachineInitiator = null;
    }

    internal void SetArgument(in CoroutineContext coroutineContext)
    {
        _coroutineContext = coroutineContext;
    }

    public void SetException(Exception e)
    {
        _builder.SetException(e);
    }

    public void SetResult()
    {
        _coroutineContext.OnStoppingCoroutine();
        _builder.SetResult();
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
        AsyncCoroutineMethodBuilderCore.ProcessAwaiterBeforeAwaitingOnCompleted(ref awaiter, ref _coroutineContext);
        _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        _builder.SetStateMachine(stateMachine);
    }
}
