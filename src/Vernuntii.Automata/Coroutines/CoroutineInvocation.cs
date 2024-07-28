using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public readonly struct CoroutineInvocationArgumentReceiver()
{
    public void ReceiveArgument<T>(T argument) {
        ;
    }
}

public delegate void CoroutineInvocationArgumentReceiverAcceptor(in CoroutineInvocationArgumentReceiver argumentReceiver);

public readonly struct CoroutineInvocation(in ValueTask task, CoroutineInvocationArgumentReceiverAcceptor argumentReceiverAcceptor)
{
    private readonly ValueTask _task = task;
    private readonly CoroutineInvocationArgumentReceiverAcceptor _argumentReceiverAcceptor = argumentReceiverAcceptor;

    public CoroutineInvocationAwaiter GetAwaiter()
    {
        return new CoroutineInvocationAwaiter(_task.GetAwaiter(), _argumentReceiverAcceptor);
    }

    public readonly struct CoroutineInvocationAwaiter(
        ValueTaskAwaiter awaiter,
        in CoroutineInvocationArgumentReceiverAcceptor argumentReceiverAcceptor) : ICriticalNotifyCompletion, ICoroutineInvocationAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ValueTaskAwaiter _awaiter = awaiter;
        private readonly IntPtr _builder = IntPtr.Zero;
        private readonly bool _isChildCoroutine = false;

        internal readonly CoroutineInvocationArgumentReceiverAcceptor ArgumentReceiverAcceptor { get; } = argumentReceiverAcceptor;

        public void GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}

public unsafe readonly struct ConfiguredAwaitableCoroutineInvocation(in ConfiguredValueTaskAwaitable task, CoroutineInvocationArgumentReceiverAcceptor argumentReceiverAcceptor)
{
    private readonly ConfiguredValueTaskAwaitable _task = task;
    private readonly CoroutineInvocationArgumentReceiverAcceptor _argumentReceiverAcceptor = argumentReceiverAcceptor;

    public ConfiguredCoroutineInvocationAwaiter GetAwaiter() => new ConfiguredCoroutineInvocationAwaiter(_task.GetAwaiter(), _argumentReceiverAcceptor);

    public readonly struct ConfiguredCoroutineInvocationAwaiter(
        in ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter awaiter,
        CoroutineInvocationArgumentReceiverAcceptor argumentReceiverAcceptor) : ICriticalNotifyCompletion, ICoroutineInvocationAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter _awaiter = awaiter;
        private readonly CoroutineInvocationArgumentReceiverAcceptor _argumentReceiverAcceptor = argumentReceiverAcceptor;

        public void GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}
