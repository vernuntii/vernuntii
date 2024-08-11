using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public partial struct CoroutineMethodBuilder
{
    public static CoroutineMethodBuilder Create()
    {
        return new CoroutineMethodBuilder();
    }

    public unsafe Coroutine Task {
        get {
            fixed (CoroutineMethodBuilder* builder = &this) {
                var stateMachineBox = _stateMachineBox ??= CoroutineMethodBuilder<VoidCoroutineResult>.CreateWeaklyTyedStateMachineBox();
                _coroutineNode.SetResultStateMachine(stateMachineBox);
                return new Coroutine(new ValueTask(stateMachineBox, stateMachineBox.Version), builder);
            }
        }
    }

    private CoroutineStackNode _coroutineNode;
    private Action? _stateMachineInitiator;
    private CoroutineMethodBuilder<VoidCoroutineResult>.CoroutineStateMachineBox _stateMachineBox;

    internal void SetCoroutineNode(ref CoroutineStackNode parentNode)
    {
        parentNode.InitializeChildCoroutine(ref _coroutineNode);
    }

    public unsafe void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        _ = CoroutineMethodBuilder<VoidCoroutineResult>.GetStateMachineBox(ref stateMachine, ref _stateMachineBox);
        //_stateMachineInitiator = stateMachine.MoveNext;
    }

    //[DebuggerStepThrough]
    internal unsafe void Start()
    {
        _coroutineNode.Start();
        Unsafe.As<ICoroutineStateMachineBox>(_stateMachineBox).MoveNext();
        //_stateMachineInitiator?.Invoke();
        //_stateMachineInitiator = null;
    }

    public void SetException(Exception e)
    {
        _stateMachineBox.SetException(e);
        _coroutineNode.Stop();
    }

    public void SetResult()
    {
        _stateMachineBox.SetResult(default);
        _coroutineNode.Stop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine => 
        CoroutineMethodBuilder<VoidCoroutineResult>.AwaitOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        CoroutineMethodBuilderCore.ProcessAwaiterBeforeAwaitingOnCompleted(ref awaiter, ref _coroutineNode);
        CoroutineMethodBuilder<VoidCoroutineResult>.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine) => throw new NotImplementedException();
}
