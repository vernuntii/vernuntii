namespace Vernuntii.Reactive.Coroutines;

public interface ICoroutineExecutor
{
    void Start(CoroutineDefinition coroutine, CancellationToken cancellationToken = default);
    Task WhenAll();
}
