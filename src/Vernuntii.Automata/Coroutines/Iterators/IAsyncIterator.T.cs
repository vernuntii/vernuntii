namespace Vernuntii.Coroutines.Iterators;

internal interface IAsyncIterator<TReturnResult>
{
    public object Current { get; }
    public ValueTask<bool> MoveNextAsync();
    public void YieldReturn<TYieldReturnResult>(TYieldReturnResult result);
    public void Return(TReturnResult result);
    public void Throw(Exception e);
    public Coroutine<TReturnResult>.CoroutineAwaiter GetAwaiter();
}
