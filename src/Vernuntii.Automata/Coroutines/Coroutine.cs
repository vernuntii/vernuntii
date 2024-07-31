using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public readonly struct CoroutineArgumentReceiver()
{
    public void ReceiveArgument<T>(in T argument)
    {
        ;
    }
}

public delegate void CoroutineArgumentReceiverAcceptor(in CoroutineArgumentReceiver argumentReceiver);

[AsyncMethodBuilder(typeof(AsyncCoroutineMethodBuilder))]
public unsafe struct Coroutine
{
    private readonly ValueTask _task;
    private readonly AsyncCoroutineMethodBuilder* _builder;
    private readonly CoroutineArgumentReceiverAcceptor? _argumentReceiverAcceptor;

    public Coroutine(in ValueTask task, CoroutineArgumentReceiverAcceptor argumentReceiverAcceptor)
    {
        _task = task;
        _argumentReceiverAcceptor = argumentReceiverAcceptor;
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

    public CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverAcceptor);

    public ConfiguredAwaitableCoroutine ConfigureAwait(bool continueOnCapturedContext) =>
        new ConfiguredAwaitableCoroutine(_task.ConfigureAwait(continueOnCapturedContext), _builder, _argumentReceiverAcceptor);

    public readonly struct CoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ValueTaskAwaiter _awaiter;
        private readonly AsyncCoroutineMethodBuilder* _builder;
        private readonly CoroutineArgumentReceiverAcceptor? _argumentReceiverAcceptor;

        readonly bool ICoroutineAwaiter.IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;
        readonly CoroutineArgumentReceiverAcceptor? ICoroutineAwaiter.ArgumentReceiverAcceptor => _argumentReceiverAcceptor;

        internal CoroutineAwaiter(in ValueTaskAwaiter awaiter, in AsyncCoroutineMethodBuilder* builder, CoroutineArgumentReceiverAcceptor? argumentReceiverAcceptor)
        {
            _awaiter = awaiter;
            _builder = builder;
            _argumentReceiverAcceptor = argumentReceiverAcceptor;
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
    private readonly CoroutineArgumentReceiverAcceptor? _argumentReceiverAcceptor;

    internal ConfiguredAwaitableCoroutine(in ConfiguredValueTaskAwaitable task, in AsyncCoroutineMethodBuilder* builder, CoroutineArgumentReceiverAcceptor? argumentReceiverAcceptor)
    {
        _task = task;
        _builder = builder;
        _argumentReceiverAcceptor = argumentReceiverAcceptor;
    }

    internal void PropagateCoroutineNode(ref CoroutineStackNode coroutineNode)
    {
        _builder->SetCoroutineNode(ref coroutineNode);
    }

    internal void StartStateMachine()
    {
        _builder->Start();
    }

    public ConfiguredCoroutineAwaiter GetAwaiter() => new ConfiguredCoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverAcceptor);

    public readonly struct ConfiguredCoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter _awaiter;
        private readonly AsyncCoroutineMethodBuilder* _builder;
        private readonly CoroutineArgumentReceiverAcceptor? _argumentReceiverAcceptor;

        readonly bool ICoroutineAwaiter.IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;
        readonly CoroutineArgumentReceiverAcceptor? ICoroutineAwaiter.ArgumentReceiverAcceptor => _argumentReceiverAcceptor;

        internal ConfiguredCoroutineAwaiter(
            in ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter awaiter,
            in AsyncCoroutineMethodBuilder* builder,
            CoroutineArgumentReceiverAcceptor? argumentReceiverAcceptor)
        {
            _awaiter = awaiter;
            _builder = builder;
            _argumentReceiverAcceptor = argumentReceiverAcceptor;
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
