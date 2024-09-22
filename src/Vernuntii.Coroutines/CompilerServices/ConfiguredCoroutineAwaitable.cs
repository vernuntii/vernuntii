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

    public readonly ConfiguredCoroutineAwaiter GetAwaiter() => new(_task.GetAwaiter(), _coroutineActioner, _coroutineAction);
}
