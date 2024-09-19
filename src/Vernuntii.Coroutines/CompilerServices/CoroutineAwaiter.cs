using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines.CompilerServices;

public struct CoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter, IRelativeCoroutineAwaiter
{
    public readonly bool IsCompleted => _awaiter.IsCompleted;

    private object? _coroutineActioner;
    internal CoroutineAction _coroutineAction;
    private readonly ValueTaskAwaiter _awaiter;

    readonly object? IRelativeCoroutine.CoroutineActioner => _coroutineActioner;
    readonly CoroutineAction IRelativeCoroutine.CoroutineAction => _coroutineAction;

    internal CoroutineAwaiter(in ValueTaskAwaiter awaiter, object? coroutineActioner, CoroutineAction coroutineAction)
    {
        _awaiter = awaiter;
        _coroutineActioner = coroutineActioner;
        _coroutineAction = coroutineAction;
    }

    void IRelativeCoroutine.MarkCoroutineAsActedOn() => _coroutineAction = CoroutineAction.None;

    public readonly void GetResult() => _awaiter.GetResult();

    public void OnCompleted(Action continuation)
    {
        if (_coroutineAction != CoroutineAction.None) {
            CoroutineMethodBuilderCore.ActOnCoroutine(ref this);
        }
        _awaiter.OnCompleted(continuation);
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        if (_coroutineAction != CoroutineAction.None) {
            CoroutineMethodBuilderCore.ActOnCoroutine(ref this);
        }
        _awaiter.UnsafeOnCompleted(continuation);
    }

    readonly void IRelativeCoroutineAwaiter.ReplaceStateMachineCoroutineAwaiter<TStateMachine>(ref TStateMachine stateMachine, ref SuspensionPoint suspensionPoint)
    {
        ref var coroutineAwaiter = ref CoroutineStateMachineCoroutineAwaiterAccessor<TStateMachine, CoroutineAwaiter>.GetCoroutineAwaiter(ref stateMachine);
    }
}
