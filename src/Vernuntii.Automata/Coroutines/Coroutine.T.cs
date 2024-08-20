using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

[AsyncMethodBuilder(typeof(CoroutineMethodBuilder<>))]
public partial struct Coroutine<TResult> : IEntryCoroutine
{
    internal readonly bool IsChildCoroutine => _builder is not null;

    internal ValueTask<TResult> _task;
    private ICoroutineMethodBuilderBox? _builder;
    private CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    readonly bool ICoroutine.IsChildCoroutine => IsChildCoroutine;
    readonly bool ICoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

    public Coroutine(in ValueTask<TResult> task)
    {
        _task = task;
    }

    public Coroutine(in ValueTask<TResult> task, CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _task = task;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    internal Coroutine(in ValueTask<TResult> task, ICoroutineMethodBuilderBox builder)
    {
        _task = task;
        _builder = builder;
    }

    void IChildCoroutine.InheritCoroutineNode(ref CoroutineStackNode coroutineNode)
    {
        Debug.Assert(_builder != null);
        _builder.InheritCoroutineNode(ref coroutineNode);
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

    void IEntryCoroutine.MarkCoroutineAsHandled()
    {
        _builder = null;
        _argumentReceiverDelegate = null;
    }

    public readonly CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverDelegate);

    public readonly ConfiguredAwaitableCoroutine<TResult> ConfigureAwait(bool continueOnCapturedContext) =>
        new ConfiguredAwaitableCoroutine<TResult>(_task.ConfigureAwait(continueOnCapturedContext), _builder, _argumentReceiverDelegate);

    public readonly struct CoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ValueTaskAwaiter<TResult> _awaiter;
        private readonly ICoroutineMethodBuilderBox? _builder;
        private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool ICoroutine.IsChildCoroutine => _builder is not null;
        readonly bool ICoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

        internal CoroutineAwaiter(in ValueTaskAwaiter<TResult> awaiter, ICoroutineMethodBuilderBox? builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
        {
            _awaiter = awaiter;
            _builder = builder;
            _argumentReceiverDelegate = argumentReceiverDelegate;
        }

        void IChildCoroutine.InheritCoroutineNode(ref CoroutineStackNode coroutineNode)
        {
            Debug.Assert(_builder != null);
            _builder.InheritCoroutineNode(ref coroutineNode);
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

public readonly struct ConfiguredAwaitableCoroutine<T>
{
    private readonly ConfiguredValueTaskAwaitable<T> _task;
    private readonly ICoroutineMethodBuilderBox? _builder;
    private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    internal ConfiguredAwaitableCoroutine(in ConfiguredValueTaskAwaitable<T> task, in ICoroutineMethodBuilderBox? builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
    {
        _task = task;
        _builder = builder;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public readonly ConfiguredCoroutineAwaiter GetAwaiter() => new ConfiguredCoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverDelegate);

    public readonly struct ConfiguredCoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter _awaiter;
        private readonly ICoroutineMethodBuilderBox? _builder;
        private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool ICoroutine.IsChildCoroutine => _builder is not null;
        readonly bool ICoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

        internal ConfiguredCoroutineAwaiter(
            in ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter awaiter,
            in ICoroutineMethodBuilderBox? builder,
            CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
        {
            _awaiter = awaiter;
            _builder = builder;
            _argumentReceiverDelegate = argumentReceiverDelegate;
        }

        void IChildCoroutine.InheritCoroutineNode(ref CoroutineStackNode coroutineNode)
        {
            Debug.Assert(_builder != null);
            _builder.InheritCoroutineNode(ref coroutineNode);
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

        public T GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}
