using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public struct AsyncCoroutineMethodBuilder
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void StartChildCoroutine<TAwaiter>(ref TAwaiter awaiter, in int argument)
    {
        if (null != (object?)default(TAwaiter) && awaiter is ICoroutineAwaiter) {
            var coroutineAwaiter = Unsafe.As<TAwaiter, Coroutine.CoroutineAwaiter>(ref awaiter);
            coroutineAwaiter.PropagateCoroutineArgument(argument);
            coroutineAwaiter.StartStateMachine();
        }
    }

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
    internal unsafe Action? _stateMachineStarter;
    private int _argument;

    public unsafe void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        _stateMachineStarter = stateMachine.MoveNext;
    }

    internal unsafe void Start()
    {
        _stateMachineStarter?.Invoke();
        _stateMachineStarter = null;
    }

    public void SetArgument(in int argument)
    {
        _argument = argument;
    }

    public void SetException(Exception e) => _builder.SetException(e);

    public void SetResult()
    {
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
        StartChildCoroutine(ref awaiter, _argument);
        _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        _builder.SetStateMachine(stateMachine);
    }
}
