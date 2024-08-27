namespace Vernuntii.Coroutines.Iterators;

public interface IAsyncIterationCompletionSource
{
    void SetResult<TResult>(TResult result);
    void SetException(Exception e);
}
