using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines.CompilerServices;

public readonly struct ConfiguredCoroutineAwaitable<TResult>
{
    private readonly IChildCoroutine? _builder;
    private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;
    private readonly ConfiguredValueTaskAwaitable<TResult> _task;

    internal ConfiguredCoroutineAwaitable(in ConfiguredValueTaskAwaitable<TResult> task, in IChildCoroutine? builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
    {
        _task = task;
        _builder = builder;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public readonly ConfiguredCoroutineAwaiter GetAwaiter() => new ConfiguredCoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverDelegate);

    public readonly struct ConfiguredCoroutineAwaiter : ICriticalNotifyCompletion, IRelativeCoroutineAwaiter, ICoroutineAwaiter<TResult>
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        internal readonly ConfiguredValueTaskAwaitable<TResult>.ConfiguredValueTaskAwaiter _awaiter;
        internal readonly IChildCoroutine? _builder;
        internal readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool IRelativeCoroutine.IsChildCoroutine => _builder is not null;
        readonly bool IRelativeCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

        internal ConfiguredCoroutineAwaiter(
            in ConfiguredValueTaskAwaitable<TResult>.ConfiguredValueTaskAwaiter awaiter,
            in IChildCoroutine? builder,
            CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
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
            _argumentReceiverDelegate(ref argumentReceiver);
        }

        public TResult GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}
