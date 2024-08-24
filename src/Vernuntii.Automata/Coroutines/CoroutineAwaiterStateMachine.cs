using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal interface ICoroutineAwaiterMethodBuilder
{
    bool IsCompleted { get; }
    void AwaitUnsafeOnCompleted();
    void GetResult();
    void SetException(Exception e);
    void SetResult();
}

internal struct CoroutineAwaiterMethodBuilder : ICoroutineAwaiterMethodBuilder
{
    public readonly bool IsCompleted => _awaiter.IsCompleted;

    public readonly ConfiguredAwaitableCoroutine.ConfiguredCoroutineAwaiter _awaiter;
    public readonly CoroutineMethodBuilder<VoidResult>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder>> _stateMachineBox;

    public CoroutineAwaiterMethodBuilder(
        in ConfiguredAwaitableCoroutine.ConfiguredCoroutineAwaiter awaiter,
        CoroutineMethodBuilder<VoidResult>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder>> stateMachineBox)
    {
        _awaiter = awaiter;
        _stateMachineBox = stateMachineBox;
    }

    public void AwaitUnsafeOnCompleted() => _awaiter.UnsafeOnCompleted(_stateMachineBox.MoveNextAction);

    public void SetException(Exception e) => _stateMachineBox.SetException(e);

    public void GetResult() => _awaiter.GetResult();

    public void SetResult() => _stateMachineBox.SetResult(default);
}

internal struct CoroutineAwaiterMethodBuilder<T> : ICoroutineAwaiterMethodBuilder
{
    public readonly bool IsCompleted => _awaiter.IsCompleted;

    private readonly ConfiguredAwaitableCoroutine<T>.ConfiguredCoroutineAwaiter _awaiter;
    private readonly CoroutineMethodBuilder<T>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<T>>> _stateMachineBox;
    private T _result;

    public CoroutineAwaiterMethodBuilder(
        in ConfiguredAwaitableCoroutine<T>.ConfiguredCoroutineAwaiter awaiter,
        CoroutineMethodBuilder<T>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<T>>> stateMachineBox)
    {
        _awaiter = awaiter;
        _stateMachineBox = stateMachineBox;
        _result = default!;
    }

    public void AwaitUnsafeOnCompleted() => _awaiter.UnsafeOnCompleted(_stateMachineBox.MoveNextAction);

    public void SetException(Exception e) => _stateMachineBox.SetException(e);

    public void GetResult() => _result = _awaiter.GetResult();

    public void SetResult() => _stateMachineBox.SetResult(_result);
}

internal struct CoroutineAwaiterStateMachine<TBuilder> : IAsyncStateMachine
    where TBuilder : struct, ICoroutineAwaiterMethodBuilder
{
    public int State; // Externally set to -1

    private TBuilder _builder;

    public CoroutineAwaiterStateMachine(TBuilder builder)
    {
        _builder = builder;
    }

    void IAsyncStateMachine.MoveNext()
    {
        var state = State;

        try {
            int newState;
            TBuilder builder;

            if (state != 0) {
                State = newState = 0;
                if (!_builder.IsCompleted) {
                    _builder.AwaitUnsafeOnCompleted();
                    return;
                }
            }
            _builder.GetResult();
        } catch (Exception error) {
            State = -2;
            _builder.SetException(error);
        }

        State = -2;
        _builder.SetResult();
    }

    void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine) => throw new NotImplementedException();
}
