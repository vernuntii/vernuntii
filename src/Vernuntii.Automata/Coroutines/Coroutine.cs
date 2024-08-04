using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

[AsyncMethodBuilder(typeof(AsyncCoroutineMethodBuilder))]
public unsafe struct Coroutine
{
    private readonly ValueTask _task;
    private readonly AsyncCoroutineMethodBuilder* _builder;
    private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    public Coroutine(in ValueTask task, CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _task = task;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public Coroutine(CoroutineArgumentReceiverDelegate argumentReceiverDelegate)
    {
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    internal Coroutine(in ValueTask task, in AsyncCoroutineMethodBuilder* builder)
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

    public readonly struct CoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ValueTaskAwaiter _awaiter;
        private readonly AsyncCoroutineMethodBuilder* _builder;
        private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool ICoroutineAwaiter.IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;
        readonly CoroutineArgumentReceiverDelegate? ICoroutineAwaiter.ArgumentReceiverDelegate => _argumentReceiverDelegate;

        internal CoroutineAwaiter(in ValueTaskAwaiter awaiter, in AsyncCoroutineMethodBuilder* builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
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
    }
}

public unsafe readonly struct ConfiguredAwaitableCoroutine
{
    private readonly ConfiguredValueTaskAwaitable _task;
    private readonly AsyncCoroutineMethodBuilder* _builder;
    private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

    internal ConfiguredAwaitableCoroutine(in ConfiguredValueTaskAwaitable task, in AsyncCoroutineMethodBuilder* builder, CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
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

        private readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter _awaiter;
        private readonly AsyncCoroutineMethodBuilder* _builder;
        private readonly CoroutineArgumentReceiverDelegate? _argumentReceiverDelegate;

        readonly bool ICoroutineAwaiter.IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;
        readonly CoroutineArgumentReceiverDelegate? ICoroutineAwaiter.ArgumentReceiverDelegate => _argumentReceiverDelegate;

        internal ConfiguredCoroutineAwaiter(
            in ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter awaiter,
            in AsyncCoroutineMethodBuilder* builder,
            CoroutineArgumentReceiverDelegate? argumentReceiverDelegate)
        {
            _awaiter = awaiter;
            _builder = builder;
            _argumentReceiverDelegate = argumentReceiverDelegate;
        }

        internal void PropagateCoroutineScpe(ref CoroutineStackNode coroutineNode)
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
    }
}
