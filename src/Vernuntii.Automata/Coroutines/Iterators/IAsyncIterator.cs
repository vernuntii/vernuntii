namespace Vernuntii.Coroutines.Generators;

public interface IAsyncIterator
{
    public Coroutine<AsyncIteratorResultObject> Next();
    public Coroutine<AsyncIteratorResultObject> Next<TNextResult>(TNextResult nextResult);
    public Coroutine Return();
    public Coroutine Return(Exception e);
}
