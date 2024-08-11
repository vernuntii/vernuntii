using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

[AsyncMethodBuilder(typeof(CoroutineMethodBuilder))]
public unsafe struct Coroutine
{
    internal ValueTask _task;
    private readonly CoroutineMethodBuilder* _builder;
    private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    public Coroutine(in ValueTask task)
    {
        _task = task;
    }

    public Coroutine(in ValueTask task, CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _task = task;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    internal Coroutine(in ValueTask task, in CoroutineMethodBuilder* builder)
    {
        _task = task;
        _builder = builder;
    }

    internal void PropagateCoroutineNode(ref CoroutineStackNode coroutineNode)
    {
        _builder->SetCoroutineNode(ref coroutineNode);
    }

    internal void StartStateMachine()
    {
        _builder->Start();
    }

    public CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverDelegate);

    public ConfiguredAwaitableCoroutine ConfigureAwait(bool continueOnCapturedContext) =>
        new ConfiguredAwaitableCoroutine(_task.ConfigureAwait(continueOnCapturedContext), _builder, _argumentReceiverDelegate);

    public readonly struct CoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter, ICoroutineStateMachineBoxAwareAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ValueTaskAwaiter _awaiter;
        private readonly CoroutineMethodBuilder* _builder;
        private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool ICoroutineAwaiter.IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;
        readonly CoroutineArgumentReceiverDelegate? ICoroutineAwaiter.ArgumentReceiverDelegate => _argumentReceiverDelegate;

        internal CoroutineAwaiter(in ValueTaskAwaiter awaiter, in CoroutineMethodBuilder* builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
        {
            _awaiter = awaiter;
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

        public void GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);

        void ICoroutineStateMachineBoxAwareAwaiter.AwaitUnsafeOnCompleted(ICoroutineStateMachineBox box) => _awaiter.UnsafeOnCompleted(box.MoveNextAction);
    }
}

public unsafe readonly struct ConfiguredAwaitableCoroutine
{
    private readonly ConfiguredValueTaskAwaitable _task;
    private readonly CoroutineMethodBuilder* _builder;
    private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    internal ConfiguredAwaitableCoroutine(in ConfiguredValueTaskAwaitable task, in CoroutineMethodBuilder* builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
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

    public readonly struct ConfiguredCoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter, ICoroutineStateMachineBoxAwareAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter _awaiter;
        private readonly CoroutineMethodBuilder* _builder;
        private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool ICoroutineAwaiter.IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;
        readonly CoroutineArgumentReceiverDelegate? ICoroutineAwaiter.ArgumentReceiverDelegate => _argumentReceiverDelegate;

        internal ConfiguredCoroutineAwaiter(
            in ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter awaiter,
            in CoroutineMethodBuilder* builder,
            CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
        {
            _awaiter = awaiter;
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

        public void GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);

        void ICoroutineStateMachineBoxAwareAwaiter.AwaitUnsafeOnCompleted(ICoroutineStateMachineBox box) => _awaiter.UnsafeOnCompleted(box.MoveNextAction);
    }
}
