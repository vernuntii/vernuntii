namespace Vernuntii.Coroutines;

public interface ICoroutineCompletionSource
{
    void SetResult<TResult>(TResult result);
    void SetException(Exception e);

    /// <summary>
    /// Creates a new and indepdenent instance of derived type.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    ICoroutineCompletionSource CreateNew(out short token);
}
