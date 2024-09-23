namespace Vernuntii.Coroutines;

public interface ICoroutineCompletionSource
{
    internal void SetResult<TResult>(TResult result) => throw new NotImplementedException();
    internal void SetException(Exception e) => throw new NotImplementedException();
}
