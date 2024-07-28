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

    internal void PropagateCoroutineScope(in CoroutineScope coroutineScope)
    {
        _builder->SetArgument(coroutineScope);
    }

    public CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(_task.GetAwaiter(), _builder, isChildCoroutine: true);

    public ConfiguredAwaitableCoroutine ConfigureAwait(bool continueOnCapturedContext) =>
        new ConfiguredAwaitableCoroutine(
            _task.ConfigureAwait(continueOnCapturedContext),
            _builder);

    public readonly struct CoroutineAwaiter(in ValueTaskAwaiter awaiter, in AsyncCoroutineMethodBuilder* builder, bool isChildCoroutine) : ICriticalNotifyCompletion, ICoroutineInvocationAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ValueTaskAwaiter _awaiter = awaiter;
        private readonly AsyncCoroutineMethodBuilder* _builder = builder;
        private readonly bool _isChildCoroutine = isChildCoroutine;

        readonly bool ICoroutineInvocationAwaiter.IsChildCoroutine => _isChildCoroutine;

        internal void StartStateMachine()
        {
            _builder->Start();
        }

        internal void PropagateCoroutineScope(in CoroutineScope coroutineScope)
        {
            _builder->SetArgument(coroutineScope);
        }

        public void GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}

public unsafe readonly struct ConfiguredAwaitableCoroutine(in ConfiguredValueTaskAwaitable task, in AsyncCoroutineMethodBuilder* builder)
{
    private readonly ConfiguredValueTaskAwaitable _task = task;
    private readonly AsyncCoroutineMethodBuilder* _builder = builder;

    internal void StartStateMachine()
    {
        _builder->Start();
    }

    internal void PropagateCoroutineScope(in CoroutineScope coroutineScope)
    {
        _builder->SetArgument(coroutineScope);
    }

    public ConfiguredCoroutineAwaiter GetAwaiter() => new ConfiguredCoroutineAwaiter(_task.GetAwaiter(), _builder);

    public readonly struct ConfiguredCoroutineAwaiter(
        in ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter awaiter,
        in AsyncCoroutineMethodBuilder* builder) : ICriticalNotifyCompletion, ICoroutineInvocationAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        private readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter _awaiter = awaiter;
        private readonly AsyncCoroutineMethodBuilder* _builder = builder;

        internal void StartStateMachine()
        {
            _builder->Start();
        }

        internal void PropagateCoroutineScpe(in CoroutineScope coroutineScope)
        {
            _builder->SetArgument(coroutineScope);
        }

        public void GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}
