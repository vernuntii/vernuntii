using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

[AsyncMethodBuilder(typeof(CoroutineMethodBuilder))]
public unsafe partial struct Coroutine : IEntryCoroutine, IEquatable<Coroutine>
{
    internal readonly bool IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;

    internal ValueTask _task;
    internal CoroutineMethodBuilder* _builder;
    internal CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    readonly bool IChildCoroutine.IsChildCoroutine => IsChildCoroutine;
    readonly bool ISiblingCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

    public Coroutine(in ValueTask task)
    {
        _task = task;
    }

    public Coroutine(in ValueTask task, CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _task = task;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    internal Coroutine(in ValueTask task, CoroutineMethodBuilder* builder)
    {
        _task = task;
        _builder = builder;
    }

    void IChildCoroutine.PropagateCoroutineNode(ref CoroutineStackNode coroutineNode)
    {
        _builder->SetCoroutineNode(ref coroutineNode);
    }

    void IChildCoroutine.StartStateMachine()
    {
        _builder->Start();
    }

    void ISiblingCoroutine.AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        Debug.Assert(_argumentReceiverDelegate is not null);
        _argumentReceiverDelegate(ref argumentReceiver);
    }

    unsafe void IEntryCoroutine.MarkCoroutineAsHandled()
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
        private readonly CoroutineMethodBuilder* _builder;
        private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool IChildCoroutine.IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;
        readonly bool ISiblingCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

        internal CoroutineAwaiter(in ValueTaskAwaiter awaiter, CoroutineMethodBuilder* builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
        {
            _awaiter = awaiter;
            _builder = builder;
            _argumentReceiverDelegate = argumentReceiverDelegate;
        }

        void IChildCoroutine.PropagateCoroutineNode(ref CoroutineStackNode coroutineNode)
        {
            _builder->SetCoroutineNode(ref coroutineNode);
        }

        void IChildCoroutine.StartStateMachine()
        {
            _builder->Start();
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

public unsafe readonly struct ConfiguredAwaitableCoroutine
{
    private readonly ConfiguredValueTaskAwaitable _task;
    private readonly CoroutineMethodBuilder* _builder;
    private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    internal ConfiguredAwaitableCoroutine(in ConfiguredValueTaskAwaitable task, CoroutineMethodBuilder* builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
    {
        _task = task;
        _builder = builder;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    internal void PropagateCoroutineNode(ref CoroutineStackNode coroutineNode)
    {
        _builder->SetCoroutineNode(ref coroutineNode);
    }

    internal void StartStateMachine()
    {
        _builder->Start();
    }

    public ConfiguredCoroutineAwaiter GetAwaiter() => new ConfiguredCoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverDelegate);

    public readonly struct ConfiguredCoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        internal readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter _awaiter;
        internal readonly CoroutineMethodBuilder* _builder;
        internal readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool IChildCoroutine.IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;
        readonly bool ISiblingCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

        internal ConfiguredCoroutineAwaiter(
            in ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter awaiter,
            CoroutineMethodBuilder* builder,
            CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
        {
            _awaiter = awaiter;
            _builder = builder;
            _argumentReceiverDelegate = argumentReceiverDelegate;
        }

        void IChildCoroutine.PropagateCoroutineNode(ref CoroutineStackNode coroutineNode)
        {
            _builder->SetCoroutineNode(ref coroutineNode);
        }

        void IChildCoroutine.StartStateMachine()
        {
            _builder->Start();
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
