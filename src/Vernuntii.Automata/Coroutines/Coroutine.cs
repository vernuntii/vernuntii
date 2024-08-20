using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

[AsyncMethodBuilder(typeof(CoroutineMethodBuilder))]
public partial struct Coroutine : IEntryCoroutine, IEquatable<Coroutine>
{
    internal readonly bool IsChildCoroutine => _builder is not null;

    internal ValueTask _task;
    internal ICoroutineMethodBuilderBox? _builder;
    internal CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    readonly bool ICoroutine.IsChildCoroutine => IsChildCoroutine;
    readonly bool ICoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

    public Coroutine(in ValueTask task)
    {
        _task = task;
    }

    public Coroutine(in ValueTask task, CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _task = task;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    internal Coroutine(in ValueTask task, ICoroutineMethodBuilderBox builder)
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

    public CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverDelegate);

    public ConfiguredAwaitableCoroutine ConfigureAwait(bool continueOnCapturedContext) =>
        new ConfiguredAwaitableCoroutine(_task.ConfigureAwait(continueOnCapturedContext), _builder, _argumentReceiverDelegate);

    bool IEquatable<Coroutine>.Equals(Coroutine other) => CoroutineEqualityComparer.Equal(in this, in other);

    public readonly struct CoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ValueTaskAwaiter _awaiter;
        private readonly ICoroutineMethodBuilderBox? _builder;
        private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool ICoroutine.IsChildCoroutine => _builder is not null;
        readonly bool ICoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

        internal CoroutineAwaiter(in ValueTaskAwaiter awaiter, ICoroutineMethodBuilderBox? builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
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

        public void GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}

public readonly struct ConfiguredAwaitableCoroutine
{
    private readonly ConfiguredValueTaskAwaitable _task;
    private readonly ICoroutineMethodBuilderBox? _builder;
    private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    internal ConfiguredAwaitableCoroutine(in ConfiguredValueTaskAwaitable task, ICoroutineMethodBuilderBox? builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
    {
        _task = task;
        _builder = builder;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public ConfiguredCoroutineAwaiter GetAwaiter() => new ConfiguredCoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverDelegate);

    public readonly struct ConfiguredCoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        internal readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter _awaiter;
        internal readonly ICoroutineMethodBuilderBox? _builder;
        internal readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool ICoroutine.IsChildCoroutine => _builder is not null;
        readonly bool ICoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

        internal ConfiguredCoroutineAwaiter(
            in ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter awaiter,
            ICoroutineMethodBuilderBox? builder,
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

        public void GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}
