namespace Vernuntii.Coroutines.Iterators;

internal interface IAsyncIteratorCompletionSource<TResult>
{
    void SetResult(TResult result);
    void SetException(Exception e);
}
