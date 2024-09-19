using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines.CompilerServices;

public struct ConfiguredCoroutineAwaitable<TResult>
{
    private readonly object? _coroutineActioner;
    internal CoroutineAction _coroutineAction;
    private readonly ConfiguredValueTaskAwaitable<TResult> _task;

    internal ConfiguredCoroutineAwaitable(in ConfiguredValueTaskAwaitable<TResult> task, in object? coroutineActioner, CoroutineAction coroutineAction)
    {
        _task = task;
        _coroutineActioner = coroutineActioner;
        _coroutineAction = coroutineAction;
    }

    public readonly ConfiguredCoroutineAwaiter GetAwaiter() => new(_task.GetAwaiter(), _coroutineActioner, _coroutineAction);

    public struct ConfiguredCoroutineAwaiter : ICriticalNotifyCompletion, IRelativeCoroutineAwaiter, ICoroutineAwaiter<TResult>
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        internal readonly object? _coroutineActioner;
        internal CoroutineAction _coroutineAction;
        internal readonly ConfiguredValueTaskAwaitable<TResult>.ConfiguredValueTaskAwaiter _awaiter;

        readonly object? IRelativeCoroutine.CoroutineActioner => _coroutineActioner;
        readonly CoroutineAction IRelativeCoroutine.CoroutineAction => _coroutineAction;

        internal ConfiguredCoroutineAwaiter(in ConfiguredValueTaskAwaitable<TResult>.ConfiguredValueTaskAwaiter awaiter, object? coroutineActioner, CoroutineAction coroutineAction)
        {
            _awaiter = awaiter;
            _coroutineActioner = coroutineActioner;
            _coroutineAction = coroutineAction;
        }

        void IRelativeCoroutine.MarkCoroutineAsActedOn() => _coroutineAction = CoroutineAction.None;

        public TResult GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation)
        {
            if (_coroutineAction != CoroutineAction.None) {
                CoroutineMethodBuilderCore.ActOnCoroutine(ref this);
            }
            _awaiter.OnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (_coroutineAction != CoroutineAction.None) {
                CoroutineMethodBuilderCore.ActOnCoroutine(ref this);
            }
            _awaiter.UnsafeOnCompleted(continuation);
        }

        readonly void IRelativeCoroutineAwaiter.ReplaceStateMachineCoroutineAwaiter<TStateMachine>(ref TStateMachine stateMachine, ref SuspensionPoint suspensionPoint)
        {
            ref var coroutineAwaiter = ref CoroutineStateMachineCoroutineAwaiterAccessor<TStateMachine, ConfiguredCoroutineAwaiter>.GetCoroutineAwaiter(ref stateMachine);
        }
    }
}
