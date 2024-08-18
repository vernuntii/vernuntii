using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

[AsyncMethodBuilder(typeof(CoroutineMethodBuilder<>))]
public unsafe partial struct Coroutine<TResult> : IEntryCoroutine
{
    internal readonly bool IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;

    internal ValueTask<TResult> _task;
    private CoroutineMethodBuilder<TResult>* _builder;
    private CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    readonly bool IChildCoroutine.IsChildCoroutine => IsChildCoroutine;
    readonly bool ISiblingCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

    public Coroutine(in ValueTask<TResult> task)
    {
        _task = task;
    }

    public Coroutine(in ValueTask<TResult> task, CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _task = task;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    internal Coroutine(in ValueTask<TResult> task, CoroutineMethodBuilder<TResult>* builder)
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

    public readonly CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverDelegate);

    public readonly ConfiguredAwaitableCoroutine<TResult> ConfigureAwait(bool continueOnCapturedContext) =>
        new ConfiguredAwaitableCoroutine<TResult>(_task.ConfigureAwait(continueOnCapturedContext), _builder, _argumentReceiverDelegate);

    public readonly struct CoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ValueTaskAwaiter<TResult> _awaiter;
        private readonly CoroutineMethodBuilder<TResult>* _builder;
        private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool IChildCoroutine.IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;
        readonly bool ISiblingCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

        internal CoroutineAwaiter(in ValueTaskAwaiter<TResult> awaiter, CoroutineMethodBuilder<TResult>* builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
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

        public TResult GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}

public readonly unsafe struct ConfiguredAwaitableCoroutine<T>
{
    private readonly ConfiguredValueTaskAwaitable<T> _task;
    private readonly CoroutineMethodBuilder<T>* _builder;
    private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    internal ConfiguredAwaitableCoroutine(in ConfiguredValueTaskAwaitable<T> task, in CoroutineMethodBuilder<T>* builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
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

    public readonly ConfiguredCoroutineAwaiter GetAwaiter() => new ConfiguredCoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverDelegate);

    public readonly struct ConfiguredCoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter _awaiter;
        private readonly CoroutineMethodBuilder<T>* _builder;
        private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool IChildCoroutine.IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;
        readonly bool ISiblingCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

        public ConfiguredCoroutineAwaiter(
            in ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter awaiter,
            in CoroutineMethodBuilder<T>* builder,
            CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
        {
            _awaiter = awaiter;
            _builder = builder;
            _argumentReceiverDelegate = argumentReceiverDelegate;
        }

        void IChildCoroutine.StartStateMachine()
        {
            _builder->Start();
        }

        void IChildCoroutine.PropagateCoroutineNode(ref CoroutineStackNode coroutineNode)
        {
            _builder->SetCoroutineNode(ref coroutineNode);
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
