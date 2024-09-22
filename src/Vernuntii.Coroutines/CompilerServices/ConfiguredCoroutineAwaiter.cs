using System.Diagnostics;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines.CompilerServices;

public struct ConfiguredCoroutineAwaiter : ICriticalNotifyCompletion, IRelativeCoroutineAwaiter, ICoroutineAwaiter
{
    public readonly bool IsCompleted => _awaiter.IsCompleted;

    internal readonly object? _coroutineActioner;
    internal CoroutineAction _coroutineAction;
    internal ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter _awaiter;

    readonly object? IRelativeCoroutine.CoroutineActioner => _coroutineActioner;
    readonly CoroutineAction IRelativeCoroutine.CoroutineAction => _coroutineAction;

    internal ConfiguredCoroutineAwaiter(in ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter awaiter, object? coroutineActioner, CoroutineAction coroutineAction)
    {
        _awaiter = awaiter;
        _coroutineActioner = coroutineActioner;
        _coroutineAction = coroutineAction;
    }

    void IRelativeCoroutine.MarkCoroutineAsActedOn() => _coroutineAction = CoroutineAction.None;

    public void GetResult() => _awaiter.GetResult();

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

    readonly void IRelativeCoroutineAwaiter.RenewStateMachineCoroutineAwaiter<TStateMachine>(
        IAsyncIteratorStateMachineHolder theirStateMachineHolder,
        in SuspensionPoint ourSuspensionPoint,
        ref SuspensionPoint theirSuspensionPoint) =>
        CoroutineAwaiterCore.RenewStateMachineCoroutineAwaiter<TStateMachine, ConfiguredCoroutineAwaiter, ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter, ValueTaskAccessor, Nothing>(
            theirStateMachineHolder,
            in ourSuspensionPoint,
            ref theirSuspensionPoint,
            GetAwaiter);

    private static ref ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter GetAwaiter(ref ConfiguredCoroutineAwaiter coroutineAwaiter) => ref coroutineAwaiter._awaiter;
}
