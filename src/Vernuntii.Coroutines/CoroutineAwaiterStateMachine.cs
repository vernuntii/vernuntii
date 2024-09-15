namespace Vernuntii.Coroutines;

internal interface ICoroutineAwaiterMethodBuilder
{
    bool IsCompleted { get; }
    void AwaitUnsafeOnCompleted();
    void GetResult();
    void SetException(Exception e);
    void SetResult();
}

internal interface ICoroutineAwaiterMethodBuilder<TResult>
{
    bool IsCompleted { get; }
    void AwaitUnsafeOnCompleted();
    TResult GetResult();
    void SetException(Exception e);
    void SetResult(TResult result);
}

internal struct CoroutineAwaiterMethodBuilder<TCoroutineAwaiter> : ICoroutineAwaiterMethodBuilder
    where TCoroutineAwaiter : struct, ICriticalNotifyCompletion, ICoroutineAwaiter
{
    public readonly bool IsCompleted => _awaiter.IsCompleted;

    public readonly TCoroutineAwaiter _awaiter;
    public readonly CoroutineMethodBuilder<Nothing>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<TCoroutineAwaiter>>> _stateMachineBox;

    public CoroutineAwaiterMethodBuilder(
        in TCoroutineAwaiter awaiter,
        CoroutineMethodBuilder<Nothing>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<TCoroutineAwaiter>>> stateMachineBox)
    {
        _awaiter = awaiter;
        _stateMachineBox = stateMachineBox;
    }

    public readonly void AwaitUnsafeOnCompleted() => _awaiter.UnsafeOnCompleted(_stateMachineBox.MoveNextAction);

    public readonly void SetException(Exception e) => _stateMachineBox.SetException(e);

    public readonly void GetResult() => _awaiter.GetResult();

    public readonly void SetResult() => _stateMachineBox.SetResult(default);
}

internal struct CoroutineAwaiterMethodBuilder<TCoroutineAwaiter, TResult> : ICoroutineAwaiterMethodBuilder<TResult>
    where TCoroutineAwaiter : struct, ICriticalNotifyCompletion, ICoroutineAwaiter<TResult>
{
    public readonly bool IsCompleted => _awaiter.IsCompleted;

    private readonly TCoroutineAwaiter _awaiter;
    private readonly CoroutineMethodBuilder<TResult>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<TCoroutineAwaiter, TResult>, TResult>> _stateMachineBox;

    public CoroutineAwaiterMethodBuilder(
        in TCoroutineAwaiter awaiter,
        CoroutineMethodBuilder<TResult>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<TCoroutineAwaiter, TResult>, TResult>> stateMachineBox)
    {
        _awaiter = awaiter;
        _stateMachineBox = stateMachineBox;
    }

    public readonly void AwaitUnsafeOnCompleted() => _awaiter.UnsafeOnCompleted(_stateMachineBox.MoveNextAction);

    public readonly void SetException(Exception e) => _stateMachineBox.SetException(e);

    public TResult GetResult() => _awaiter.GetResult();

    public readonly void SetResult(TResult result) => _stateMachineBox.SetResult(result);
}

internal struct CoroutineAwaiterStateMachine<TBuilder> : IAsyncStateMachine
    where TBuilder : struct, ICoroutineAwaiterMethodBuilder
{
    internal int _state; // Externally set to -1

    private readonly TBuilder _builder;

    public CoroutineAwaiterStateMachine(in TBuilder builder)
    {
        _builder = builder;
    }

    void IAsyncStateMachine.MoveNext()
    {
        var state = _state;

        try {
            if (state != 0) {
                if (!_builder.IsCompleted) {
                    _builder.AwaitUnsafeOnCompleted();
                    return;
                }
            }
            _builder.GetResult();
        } catch (Exception error) {
            _state = -2;
            _builder.SetException(error);
            return;
        }

        _state = -2;
        _builder.SetResult();
    }

    void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine) => throw new NotImplementedException();
}

internal struct CoroutineAwaiterStateMachine<TBuilder, TResult> : IAsyncStateMachine
    where TBuilder : struct, ICoroutineAwaiterMethodBuilder<TResult>
{
    internal int _state; // Externally set to -1

    private readonly TBuilder _builder;

    public CoroutineAwaiterStateMachine(in TBuilder builder)
    {
        _builder = builder;
    }

    void IAsyncStateMachine.MoveNext()
    {
        var state = _state;
        TResult result;

        try {
            if (state != 0) {
                if (!_builder.IsCompleted) {
                    _builder.AwaitUnsafeOnCompleted();
                    return;
                }
            }
            result = _builder.GetResult();
        } catch (Exception error) {
            _state = -2;
            _builder.SetException(error);
            return;
        }

        _state = -2;
        _builder.SetResult(result);
    }

    void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine) => throw new NotImplementedException();
}
