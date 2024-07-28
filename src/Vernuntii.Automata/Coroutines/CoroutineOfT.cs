using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

[AsyncMethodBuilder(typeof(AsyncCoroutineMethodBuilder<>))]
public unsafe struct Coroutine<T>(in ValueTask<T> task, in AsyncCoroutineMethodBuilder<T>* builder)
{
    private readonly ValueTask<T> _task = task;
    private readonly AsyncCoroutineMethodBuilder<T>* _builder = builder;

    internal void StartStateMachine()
    {
        _builder->Start();
    }

    internal void PropagateCoroutineArgument(in int argument)
    {
        _builder->SetArgument(argument);
    }

    public CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(_task.GetAwaiter(), _builder);

    public ConfiguredAwaitableCoroutine<T> ConfigureAwait(bool continueOnCapturedContext) =>
        new ConfiguredAwaitableCoroutine<T>(
            _task.ConfigureAwait(continueOnCapturedContext),
            _builder);

    public struct CoroutineAwaiter(in ValueTaskAwaiter<T> awaiter, in AsyncCoroutineMethodBuilder<T>* builder) : ICriticalNotifyCompletion, ICoroutineInvocationAwaiter
    {
        private readonly ValueTaskAwaiter<T> _awaiter = awaiter;
        private readonly AsyncCoroutineMethodBuilder<T>* _builder = builder;

        public readonly bool IsCompleted => _awaiter.IsCompleted;

        internal void StartStateMachine()
        {
            _builder->Start();
        }

        internal void PropagateCoroutineArgument(in int argument)
        {
            _builder->SetArgument(argument);
        }

        public T GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}

public unsafe struct ConfiguredAwaitableCoroutine<T>(in ConfiguredValueTaskAwaitable<T> task, in AsyncCoroutineMethodBuilder<T>* builder)
{
    private readonly ConfiguredValueTaskAwaitable<T> _task = task;
    private readonly AsyncCoroutineMethodBuilder<T>* _builder = builder;

    internal void StartStateMachine()
    {
        _builder->Start();
    }

    internal void PropagateCoroutineArgument(in int argument)
    {
        _builder->SetArgument(argument);
    }

    public ConfiguredCoroutineAwaiter GetAwaiter() => new ConfiguredCoroutineAwaiter(_task.GetAwaiter(), _builder);

    public struct ConfiguredCoroutineAwaiter(
        in ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter awaiter,
        in AsyncCoroutineMethodBuilder<T>* builder) : ICriticalNotifyCompletion, ICoroutineInvocationAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter _awaiter = awaiter;
        private readonly AsyncCoroutineMethodBuilder<T>* _builder = builder;

        bool ICoroutineInvocationAwaiter.IsChildCoroutine => true;

        internal void StartStateMachine()
        {
            _builder->Start();
        }

        internal void PropagateCoroutineArgument(in int argument)
        {
            _builder->SetArgument(argument);
        }

        public T GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}
