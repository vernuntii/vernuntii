namespace Vernuntii.Coroutines.Generators;

internal interface IAsyncIterator<TResult>
{
    public Coroutine<AsyncIteratorResultObject> Next();
    public Coroutine<AsyncIteratorResultObject> Next<TNextResult>(TNextResult nextResult);
    public Coroutine Return(TResult result);
    public Coroutine Return(Exception e);
}
