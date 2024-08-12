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
                //Console.WriteLine("Task [THREAD: " + Thread.CurrentThread.ManagedThreadId + "]");
                var stateMachineBox = _stateMachineBox ??= CreateWeaklyTyedStateMachineBox();
                _coroutineNode.SetResultStateMachine(stateMachineBox);
                return new Coroutine<T>(new ValueTask<T>(stateMachineBox, stateMachineBox.Version), builder);
            }
        }
    }

    private CoroutineStackNode _coroutineNode;
    private Action? _stateMachineInitiator;
    private CoroutineStateMachineBox _stateMachineBox;

    internal void SetCoroutineNode(ref CoroutineStackNode parentNode)
    {
        parentNode.InitializeChildCoroutine(ref _coroutineNode);
    }

    public unsafe void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        Console.WriteLine("Start [THREAD: " + Thread.CurrentThread.ManagedThreadId + "]");
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
        Console.WriteLine("AwaitUnsafeOnCompleted [THREAD: " + Thread.CurrentThread.ManagedThreadId + "] [PRE]");
        CoroutineMethodBuilderCore.ProcessAwaiterBeforeAwaitingOnCompleted(ref awaiter, ref _coroutineNode);
        AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);
        Console.WriteLine("AwaitUnsafeOnCompleted [THREAD: " + Thread.CurrentThread.ManagedThreadId + "] [POST]");
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new NotImplementedException();
}
