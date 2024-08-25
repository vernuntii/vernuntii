namespace Vernuntii.Coroutines.Iterators;

internal interface IAsyncIterator<TResult>
{
    public object Current { get; }
    public Coroutine<bool> MoveNext();
    public Coroutine YieldReturn<TResult>(TResult result);
    public Coroutine Return(TResult result);
    public Coroutine Throw(Exception e);
}
