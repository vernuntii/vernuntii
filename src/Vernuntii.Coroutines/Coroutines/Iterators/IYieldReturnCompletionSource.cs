namespace Vernuntii.Coroutines.Iterators;

public interface IYieldReturnCompletionSource
{
    void SetResult<TResult>(TResult result);
    void SetException(Exception e);
}
