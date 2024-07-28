using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public readonly struct CoroutineInvocation<T>(in ValueTask<T> task, CoroutineInvocationArgumentReceiverAcceptor argumentReceiverAcceptor)
{
    private readonly ValueTask<T> _task = task;
    private readonly CoroutineInvocationArgumentReceiverAcceptor _argumentReceiverAcceptor = argumentReceiverAcceptor;

    public CoroutineInvocationAwaiter GetAwaiter()
    {
        return new CoroutineInvocationAwaiter(_task.GetAwaiter(), _argumentReceiverAcceptor);
    }

    public readonly struct CoroutineInvocationAwaiter(
        ValueTaskAwaiter<T> awaiter,
        in CoroutineInvocationArgumentReceiverAcceptor argumentReceiverAcceptor) : ICriticalNotifyCompletion, ICoroutineInvocationAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ValueTaskAwaiter<T> _awaiter = awaiter;
        private readonly IntPtr _builder = IntPtr.Zero;
        private readonly bool _isChildCoroutine = false;

        internal readonly CoroutineInvocationArgumentReceiverAcceptor ArgumentReceiverAcceptor { get; } = argumentReceiverAcceptor;

        public T GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}

public unsafe readonly struct ConfiguredAwaitableCoroutineInvocation<T>(in ConfiguredValueTaskAwaitable<T> task, CoroutineInvocationArgumentReceiverAcceptor argumentReceiverAcceptor)
{
    private readonly ConfiguredValueTaskAwaitable<T> _task = task;
    private readonly CoroutineInvocationArgumentReceiverAcceptor _argumentReceiverAcceptor = argumentReceiverAcceptor;

    public ConfiguredCoroutineInvocationAwaiter GetAwaiter() => new ConfiguredCoroutineInvocationAwaiter(_task.GetAwaiter(), _argumentReceiverAcceptor);

    public readonly struct ConfiguredCoroutineInvocationAwaiter(
        in ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter awaiter,
        CoroutineInvocationArgumentReceiverAcceptor argumentReceiverAcceptor) : ICriticalNotifyCompletion, ICoroutineInvocationAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter _awaiter = awaiter;
        private readonly CoroutineInvocationArgumentReceiverAcceptor _argumentReceiverAcceptor = argumentReceiverAcceptor;

        public T GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}
