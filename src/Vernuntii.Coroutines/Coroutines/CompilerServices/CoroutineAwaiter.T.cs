using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines.CompilerServices;

public readonly struct CoroutineAwaiter<TResult> : ICriticalNotifyCompletion, IRelativeCoroutineAwaiter, ICoroutineAwaiter<TResult>
{
    public readonly bool IsCompleted => _awaiter.IsCompleted;

    private readonly IChildCoroutine? _builder;
    private readonly ISiblingCoroutine? _argumentReceiverDelegate;
    private readonly ValueTaskAwaiter<TResult> _awaiter;

    readonly bool IRelativeCoroutine.IsChildCoroutine => _builder is not null;
    readonly bool IRelativeCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

    internal CoroutineAwaiter(in ValueTaskAwaiter<TResult> awaiter, IChildCoroutine? builder, ISiblingCoroutine? argumentReceiverDelegate)
    {
        _awaiter = awaiter;
        _builder = builder;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    void IChildCoroutine.InheritCoroutineContext(in CoroutineContext context)
    {
        Debug.Assert(_builder != null);
        _builder.InheritCoroutineContext(in context);
    }

    void IChildCoroutine.StartCoroutine()
    {
        Debug.Assert(_builder != null);
        _builder.StartCoroutine();
    }

    void ISiblingCoroutine.AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        Debug.Assert(_argumentReceiverDelegate is not null);
        _argumentReceiverDelegate.AcceptCoroutineArgumentReceiver(ref argumentReceiver);
    }

    public TResult GetResult() => _awaiter.GetResult();

    public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

    public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
}
