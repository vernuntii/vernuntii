using System.Threading.Tasks.Sources;

namespace Vernuntii.Coroutines;

internal class CoroutineCompletionSource<T> : IValueTaskSource<T>
{
    private ManualResetValueTaskSourceCore<T> _core;

    public ValueTask<T> CreateValueTask()
    {
        return new ValueTask<T>(this, _core.Version);
    }

    public T GetResult(short token)
    {
        return _core.GetResult(token);
    }

    public ValueTaskSourceStatus GetStatus(short token)
    {
        return _core.GetStatus(token);
    }

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        _core.OnCompleted(continuation, state, token, flags);
    }

    public void SetResult(T result)
    {
        _core.SetResult(result);
    }

    public void SetException(Exception exception)
    {
        _core.SetException(exception);
    }

    public void Reset()
    {
        _core.Reset();
    }
}
