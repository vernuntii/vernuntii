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

    public readonly ConfiguredCoroutineAwaiter<TResult> GetAwaiter() => new(_task.GetAwaiter(), _coroutineActioner, _coroutineAction);
}
