using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

[AsyncMethodBuilder(typeof(AsyncCoroutineMethodBuilder<>))]
public unsafe struct Coroutine<T>
{
    private readonly ValueTask<T> _task;
    private readonly AsyncCoroutineMethodBuilder<T>* _builder;
    private readonly CoroutineArgumentReceiverAcceptor? _argumentReceiverAcceptor;

    public Coroutine(in ValueTask<T> task, CoroutineArgumentReceiverAcceptor argumentReceiverAcceptor)
    {
        _task = task;
        _argumentReceiverAcceptor = argumentReceiverAcceptor;
    }

    internal Coroutine(in ValueTask<T> task, in AsyncCoroutineMethodBuilder<T>* builder)
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

    public readonly CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverAcceptor);

    public readonly ConfiguredAwaitableCoroutine<T> ConfigureAwait(bool continueOnCapturedContext) =>
        new ConfiguredAwaitableCoroutine<T>(_task.ConfigureAwait(continueOnCapturedContext), _builder, _argumentReceiverAcceptor);

    public struct CoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ValueTaskAwaiter<T> _awaiter;
        private readonly AsyncCoroutineMethodBuilder<T>* _builder;
        private readonly CoroutineArgumentReceiverAcceptor? _argumentReceiverAcceptor;

        readonly bool ICoroutineAwaiter.IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;
        readonly CoroutineArgumentReceiverAcceptor? ICoroutineAwaiter.ArgumentReceiverAcceptor => _argumentReceiverAcceptor;

        internal CoroutineAwaiter(in ValueTaskAwaiter<T> awaiter, in AsyncCoroutineMethodBuilder<T>* builder, CoroutineArgumentReceiverAcceptor? argumentReceiverAcceptor)
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

        public T GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}

public readonly unsafe struct ConfiguredAwaitableCoroutine<T>
{
    private readonly ConfiguredValueTaskAwaitable<T> _task;
    private readonly AsyncCoroutineMethodBuilder<T>* _builder;
    private readonly CoroutineArgumentReceiverAcceptor? _argumentReceiverAcceptor;

    internal ConfiguredAwaitableCoroutine(in ConfiguredValueTaskAwaitable<T> task, in AsyncCoroutineMethodBuilder<T>* builder, CoroutineArgumentReceiverAcceptor? argumentReceiverAcceptor)
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

    public readonly ConfiguredCoroutineAwaiter GetAwaiter() => new ConfiguredCoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverAcceptor);

    public readonly struct ConfiguredCoroutineAwaiter : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter _awaiter;
        private readonly AsyncCoroutineMethodBuilder<T>* _builder;
        private readonly CoroutineArgumentReceiverAcceptor? _argumentReceiverAcceptor;

        readonly bool ICoroutineAwaiter.IsChildCoroutine => (IntPtr)_builder != IntPtr.Zero;
        readonly CoroutineArgumentReceiverAcceptor? ICoroutineAwaiter.ArgumentReceiverAcceptor => _argumentReceiverAcceptor;

        public ConfiguredCoroutineAwaiter(
            in ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter awaiter,
            in AsyncCoroutineMethodBuilder<T>* builder,
            CoroutineArgumentReceiverAcceptor? argumentReceiverAcceptor)
        {
            _awaiter = awaiter;
            _builder = builder;
            _argumentReceiverAcceptor = argumentReceiverAcceptor;
        }

        internal void StartStateMachine()
        {
            _builder->Start();
        }

        internal void PropagateCoroutineNode(ref CoroutineStackNode coroutineNode)
        {
            _builder->SetCoroutineNode(ref coroutineNode);
        }

        public T GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}
