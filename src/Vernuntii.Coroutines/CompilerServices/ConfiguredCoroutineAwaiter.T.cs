using System.Diagnostics;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines.CompilerServices;

public struct ConfiguredCoroutineAwaiter<TResult> : ICriticalNotifyCompletion, IRelativeCoroutineAwaiter, ICoroutineAwaiter<TResult>
{
    public readonly bool IsCompleted => _awaiter.IsCompleted;

    internal readonly object? _coroutineActioner;
    internal CoroutineAction _coroutineAction;
    internal ConfiguredValueTaskAwaitable<TResult>.ConfiguredValueTaskAwaiter _awaiter;

    readonly object? IRelativeCoroutine.CoroutineActioner => _coroutineActioner;
    readonly CoroutineAction IRelativeCoroutine.CoroutineAction => _coroutineAction;

    internal ConfiguredCoroutineAwaiter(in ConfiguredValueTaskAwaitable<TResult>.ConfiguredValueTaskAwaiter awaiter, object? coroutineActioner, CoroutineAction coroutineAction)
    {
        _awaiter = awaiter;
        _coroutineActioner = coroutineActioner;
        _coroutineAction = coroutineAction;
    }

    void IRelativeCoroutine.MarkCoroutineAsActedOn() => _coroutineAction = CoroutineAction.None;

    public TResult GetResult() => _awaiter.GetResult();

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
        CoroutineAwaiterCore.RenewStateMachineCoroutineAwaiter<TStateMachine, ConfiguredCoroutineAwaiter<TResult>, ConfiguredValueTaskAwaitable<TResult>.ConfiguredValueTaskAwaiter, ValueTaskAccessor<TResult>, TResult>(
            theirStateMachineHolder,
            in ourSuspensionPoint,
            ref theirSuspensionPoint,
            GetAwaiter);

    private static ref ConfiguredValueTaskAwaitable<TResult>.ConfiguredValueTaskAwaiter GetAwaiter(ref ConfiguredCoroutineAwaiter<TResult> coroutineAwaiter) => ref coroutineAwaiter._awaiter;
}
