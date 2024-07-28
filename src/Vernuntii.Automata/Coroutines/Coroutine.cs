using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

[AsyncMethodBuilder(typeof(AsyncCoroutineMethodBuilder))]
public unsafe struct Coroutine(in ValueTask task, in AsyncCoroutineMethodBuilder* builder)
{
    private readonly ValueTask _task = task;
    private readonly AsyncCoroutineMethodBuilder* _builder = builder;

    internal void StartStateMachine()
    {
        _builder->Start();
    }

    internal void PropagateCoroutineArgument(in int argument)
    {
        _builder->SetArgument(argument);
    }

    public CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(_task.GetAwaiter(), _builder);

    public ConfiguredAwaitableCoroutine ConfigureAwait(bool continueOnCapturedContext) =>
        new ConfiguredAwaitableCoroutine(
            _task.ConfigureAwait(continueOnCapturedContext),
            _builder);

    public struct CoroutineAwaiter(in ValueTaskAwaiter awaiter, in AsyncCoroutineMethodBuilder* builder) : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        private readonly ValueTaskAwaiter _awaiter = awaiter;
        private readonly AsyncCoroutineMethodBuilder* _builder = builder;

        public readonly bool IsCompleted => _awaiter.IsCompleted;

        internal void StartStateMachine()
        {
            _builder->Start();
        }

        internal void PropagateCoroutineArgument(in int argument)
        {
            _builder->SetArgument(argument);
        }

        public void GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}

public unsafe struct ConfiguredAwaitableCoroutine(in ConfiguredValueTaskAwaitable task, in AsyncCoroutineMethodBuilder* builder)
{
    private readonly ConfiguredValueTaskAwaitable _task = task;
    private readonly AsyncCoroutineMethodBuilder* _builder = builder;

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
        in ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter awaiter,
        in AsyncCoroutineMethodBuilder* builder) : ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        private readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter _awaiter = awaiter;
        private readonly AsyncCoroutineMethodBuilder* _builder = builder;

        public readonly bool IsCompleted => _awaiter.IsCompleted;

        internal void StartStateMachine()
        {
            _builder->Start();
        }

        internal void PropagateCoroutineArgument(in int argument)
        {
            _builder->SetArgument(argument);
        }

        public void GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}
