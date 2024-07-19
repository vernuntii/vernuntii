namespace Vernuntii.Reactive.Coroutines;

public delegate IAsyncEnumerable<Steps.IStep> CoroutineDefinition2<T>(T t);

public interface ICoroutineExecutor
{
    void Start(CoroutineDefinition coroutine, CancellationToken cancellationToken = default);
    //void Start2<T>(CoroutineDefinition2<T> coroutine, CancellationToken cancellationToken = default);
    Task WhenAll();
}
