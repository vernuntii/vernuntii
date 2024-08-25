namespace Vernuntii.Coroutines.Iterators;

public interface IAsyncIterator
{
    public object Current { get; }
    public Coroutine<bool> MoveNext();
    public Coroutine YieldReturn<TResult>(TResult result);
    public Coroutine Return();
    public Coroutine Throw(Exception e);
}
