namespace Vernuntii.Coroutines.Iterators;

public interface IAsyncIterator
{
    public object Current { get; }
    public ValueTask<bool> MoveNextAsync();
    public void YieldReturn<TResult>(TResult result);
    public void Return();
    public void Throw(Exception e);
}
