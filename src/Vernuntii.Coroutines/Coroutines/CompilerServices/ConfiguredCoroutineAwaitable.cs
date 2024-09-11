using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines.CompilerServices;

public struct ConfiguredCoroutineAwaitable
{
    private readonly object? _coroutineActioner;
    internal CoroutineAction _coroutineAction;
    private readonly ConfiguredValueTaskAwaitable _task;

    internal ConfiguredCoroutineAwaitable(in ConfiguredValueTaskAwaitable task, object? coroutineActioner, CoroutineAction coroutineAction)
    {
        _task = task;
        _coroutineActioner = coroutineActioner;
        _coroutineAction = coroutineAction;
    }

    public ConfiguredCoroutineAwaiter GetAwaiter() => new ConfiguredCoroutineAwaiter(_task.GetAwaiter(), _coroutineActioner, _coroutineAction);

    public struct ConfiguredCoroutineAwaiter : ICriticalNotifyCompletion, IRelativeCoroutine, ICoroutineAwaiter
    {
        public readonly bool IsCompleted => _awaiter.IsCompleted;

        internal readonly object? _coroutineActioner;
        internal CoroutineAction _coroutineAction;
        internal readonly ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter _awaiter;

        readonly object? IRelativeCoroutine.CoroutineActioner => _coroutineActioner;
        readonly CoroutineAction IRelativeCoroutine.CoroutineAction => _coroutineAction;

        internal ConfiguredCoroutineAwaiter(in ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter awaiter, object? coroutineActioner, CoroutineAction coroutineAction)
        {
            _awaiter = awaiter;
            _coroutineActioner = coroutineActioner;
            _coroutineAction = coroutineAction;
        }

        void IRelativeCoroutine.MarkCoroutineAsActedOn() => _coroutineAction = CoroutineAction.Task;

        public void GetResult() => _awaiter.GetResult();

        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
    }
}
