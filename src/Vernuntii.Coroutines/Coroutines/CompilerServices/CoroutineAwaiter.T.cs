using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines.CompilerServices;

public struct CoroutineAwaiter<TResult> : ICriticalNotifyCompletion, IRelativeCoroutine, ICoroutineAwaiter<TResult>
{
    public readonly bool IsCompleted => _awaiter.IsCompleted;

    private readonly object? _coroutineActioner;
    internal CoroutineAction _coroutineAction;
    private readonly ValueTaskAwaiter<TResult> _awaiter;

    readonly object? IRelativeCoroutine.CoroutineActioner => _coroutineActioner;
    readonly CoroutineAction IRelativeCoroutine.CoroutineAction => _coroutineAction;

    internal CoroutineAwaiter(in ValueTaskAwaiter<TResult> awaiter, object? coroutineActioner, CoroutineAction coroutineAction)
    {
        _awaiter = awaiter;
        _coroutineActioner = coroutineActioner;
        _coroutineAction = coroutineAction;
    }

    void IRelativeCoroutine.MarkCoroutineAsActedOn() => _coroutineAction = CoroutineAction.Task;

    public TResult GetResult() => _awaiter.GetResult();

    public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

    public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
}
