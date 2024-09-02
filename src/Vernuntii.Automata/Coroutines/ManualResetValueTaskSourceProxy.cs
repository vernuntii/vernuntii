using System.Threading.Tasks.Sources;

namespace Vernuntii.Coroutines;

internal struct ManualResetValueTaskSourceProxy<TResult>
{
    internal ManualResetValueTaskSourceCore<TResult> _valueTaskSource;

    internal IValueTaskCompletionSource<TResult>? _asyncIteratorCompletionSource;

    public bool RunContinuationsAsynchronously {
        readonly get => _valueTaskSource.RunContinuationsAsynchronously;
        set => _valueTaskSource.RunContinuationsAsynchronously = value;
    }

    public short Version => _valueTaskSource.Version;

    public TResult GetResult(short token) => _valueTaskSource.GetResult(token);

    public ValueTaskSourceStatus GetStatus(short token) => _valueTaskSource.GetStatus(token);

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) =>
        _valueTaskSource.OnCompleted(continuation, state, token, flags);

    public void Reset()
    {
        _asyncIteratorCompletionSource = null;
        _valueTaskSource.Reset();
    }

    public void SetException(Exception error)
    {
        if (_asyncIteratorCompletionSource is null) {
            _valueTaskSource.SetException(error);
        } else {
            _asyncIteratorCompletionSource.SetException(error);
        }
    }

    public void SetResult(TResult result)
    {
        if (_asyncIteratorCompletionSource is null) {
            _valueTaskSource.SetResult(result);
        } else {
            _asyncIteratorCompletionSource.SetResult(result);
        }
    }
}
