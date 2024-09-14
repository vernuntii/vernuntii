namespace Vernuntii.Coroutines.Iterators;

public interface IYieldCompletionSource
{
    void SetResult<TResult>(TResult result);
    void SetException(Exception e);
}
