using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;

namespace Vernuntii.Coroutines;

[AsyncMethodBuilder(typeof(CoroutineMethodBuilder<>))]
public partial struct Coroutine<TResult> : IAwaiterAwareCoroutine, IEquatable<Coroutine<TResult>>
{
    internal readonly bool IsChildCoroutine => _builder is not null;

    internal ValueTask<TResult> _task;
    private ICoroutineMethodBuilderBox? _builder;
    private CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    readonly bool IRelativeCoroutine.IsChildCoroutine => IsChildCoroutine;
    readonly bool IRelativeCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

    public Coroutine(in ValueTask<TResult> task)
    {
        _task = task;
    }

    public Coroutine(IValueTaskSource<TResult> source, short token)
    {
        _task = new ValueTask<TResult>(source, token);
    }

    public Coroutine(Task<TResult> task)
    {
        _task = new ValueTask<TResult>(task);
    }

    public Coroutine(TResult result)
    {
        _task = new ValueTask<TResult>(result);
    }

    public Coroutine(in ValueTask<TResult> task, CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _task = task;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public Coroutine(IValueTaskSource<TResult> source, short token, CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _task = new ValueTask<TResult>(source, token);
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public Coroutine(Task<TResult> task, CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _task = new ValueTask<TResult>(task);
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public Coroutine(TResult result, CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _task = new ValueTask<TResult>(result);
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    internal Coroutine(in ValueTask<TResult> task, ICoroutineMethodBuilderBox builder)
    {
        _task = task;
        _builder = builder;
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

    void IAwaiterAwareCoroutine.MarkCoroutineAsHandled()
    {
        _builder = null;
        _argumentReceiverDelegate = null;
    }

    public readonly CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverDelegate);

    public readonly ConfiguredAwaitableCoroutine<TResult> ConfigureAwait(bool continueOnCapturedContext) =>
        new ConfiguredAwaitableCoroutine<TResult>(_task.ConfigureAwait(continueOnCapturedContext), _builder, _argumentReceiverDelegate);

    internal readonly Task AsTask() => _task.AsTask();

    public readonly bool Equals(Coroutine<TResult> other) => CoroutineEqualityComparer.Equals(in this, in other);

    /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="object"/>.</summary>
    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is Coroutine<TResult> && Equals((Coroutine<TResult>)obj);

    /// <summary>Returns a value indicating whether two <see cref="ValueTask"/> values are equal.</summary>
    public static bool operator ==(Coroutine<TResult> left, Coroutine<TResult> right) =>
        left.Equals(right);

    /// <summary>Returns a value indicating whether two <see cref="ValueTask"/> values are not equal.</summary>
    public static bool operator !=(Coroutine<TResult> left, Coroutine<TResult> right) =>
        !left.Equals(right);

    public readonly struct CoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ValueTaskAwaiter<TResult> _awaiter;
        private readonly ICoroutineMethodBuilderBox? _builder;
        private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool IRelativeCoroutine.IsChildCoroutine => _builder is not null;
        readonly bool IRelativeCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

        internal CoroutineAwaiter(in ValueTaskAwaiter<TResult> awaiter, ICoroutineMethodBuilderBox? builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
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

        internal readonly ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter _awaiter;
        internal readonly ICoroutineMethodBuilderBox? _builder;
        internal readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool IRelativeCoroutine.IsChildCoroutine => _builder is not null;
        readonly bool IRelativeCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

        internal ConfiguredCoroutineAwaiter(
            in ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter awaiter,
            in ICoroutineMethodBuilderBox? builder,
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

        public T GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}
