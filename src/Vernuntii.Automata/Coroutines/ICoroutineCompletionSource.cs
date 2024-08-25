namespace Vernuntii.Coroutines;

public interface ICoroutineCompletionSource
{
    void SetResult<TResult>(TResult result);
    void SetException(Exception e);
}
