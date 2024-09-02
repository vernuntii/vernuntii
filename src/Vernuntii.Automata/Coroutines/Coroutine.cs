using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;

namespace Vernuntii.Coroutines;

[AsyncMethodBuilder(typeof(CoroutineMethodBuilder))]
public partial struct Coroutine : IAwaitableCoroutine, IEquatable<Coroutine>
{
    internal readonly bool IsChildCoroutine {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            return _builder is not null;
        }
    }

    internal ValueTask _task;
    internal ICoroutineMethodBuilderBox? _builder;
    internal CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    readonly bool IRelativeCoroutine.IsChildCoroutine => IsChildCoroutine;
    readonly bool IRelativeCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

    public Coroutine(in ValueTask task)
    {
        _task = task;
    }

    public Coroutine(IValueTaskSource source, short token)
    {
        _task = new ValueTask(source, token);
    }

    public Coroutine(Task task)
    {
        _task = new ValueTask(task);
    }

    public Coroutine(in ValueTask task, CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _task = task;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public Coroutine(IValueTaskSource source, short token, CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _task = new ValueTask(source, token);
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public Coroutine(Task task, CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _task = new ValueTask(task);
    }

    internal Coroutine(in ValueTask task, ICoroutineMethodBuilderBox builder)
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

    void IAwaitableCoroutine.MarkCoroutineAsHandled()
    {
        _builder = null;
        _argumentReceiverDelegate = null;
    }

    public CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverDelegate);

    public ConfiguredAwaitableCoroutine ConfigureAwait(bool continueOnCapturedContext) =>
        new ConfiguredAwaitableCoroutine(_task.ConfigureAwait(continueOnCapturedContext), _builder, _argumentReceiverDelegate);

    public readonly bool Equals(Coroutine other) => CoroutineEqualityComparer.Equals(in this, in other);

    /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="object"/>.</summary>
    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is Coroutine && Equals((Coroutine)obj);

    /// <summary>Returns a value indicating whether two <see cref="ValueTask"/> values are equal.</summary>
    public static bool operator ==(Coroutine left, Coroutine right) =>
        left.Equals(right);

    /// <summary>Returns a value indicating whether two <see cref="ValueTask"/> values are not equal.</summary>
    public static bool operator !=(Coroutine left, Coroutine right) =>
        !left.Equals(right);

    public readonly struct CoroutineAwaiter : ICriticalNotifyCompletion, IRelativeCoroutineAwaiter, ICoroutineAwaiter
    {
        internal readonly bool IsSiblingCoroutine {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return _argumentReceiverDelegate is not null;
            }
        }

        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ValueTaskAwaiter _awaiter;
        private readonly ICoroutineMethodBuilderBox? _builder;
        private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool IRelativeCoroutine.IsChildCoroutine => _builder is not null;
        readonly bool IRelativeCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

        internal CoroutineAwaiter(in ValueTaskAwaiter awaiter, ICoroutineMethodBuilderBox? builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
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

    public readonly struct ConfiguredCoroutineAwaiter : ICriticalNotifyCompletion, IRelativeCoroutineAwaiter, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        internal readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter _awaiter;
        internal readonly ICoroutineMethodBuilderBox? _builder;
        internal readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool IRelativeCoroutine.IsChildCoroutine => _builder is not null;
        readonly bool IRelativeCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

        internal ConfiguredCoroutineAwaiter(
            in ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter awaiter,
            ICoroutineMethodBuilderBox? builder,
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

        public void GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}
