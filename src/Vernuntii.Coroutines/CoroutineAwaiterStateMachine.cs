using System.Runtime.CompilerServices;
using Vernuntii.Coroutines.CompilerServices;

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

    public readonly ConfiguredCoroutineAwaitable.ConfiguredCoroutineAwaiter _awaiter;
    public readonly CoroutineMethodBuilder<Nothing>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder>> _stateMachineBox;

    public CoroutineAwaiterMethodBuilder(
        in ConfiguredCoroutineAwaitable.ConfiguredCoroutineAwaiter awaiter,
        CoroutineMethodBuilder<Nothing>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder>> stateMachineBox)
    {
        _awaiter = awaiter;
        _stateMachineBox = stateMachineBox;
    }

    public readonly void AwaitUnsafeOnCompleted() => _awaiter.UnsafeOnCompleted(_stateMachineBox.MoveNextAction);

    public readonly void SetException(Exception e) => _stateMachineBox.SetException(e);

    public readonly void GetResult() => _awaiter.GetResult();

    public readonly void SetResult() => _stateMachineBox.SetResult(default);
}

internal struct CoroutineAwaiterMethodBuilder<T> : ICoroutineAwaiterMethodBuilder
{
    public readonly bool IsCompleted => _awaiter.IsCompleted;

    private readonly ConfiguredCoroutineAwaitable<T>.ConfiguredCoroutineAwaiter _awaiter;
    private readonly CoroutineMethodBuilder<T>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<T>>> _stateMachineBox;
    private T _result;

    public CoroutineAwaiterMethodBuilder(
        in ConfiguredCoroutineAwaitable<T>.ConfiguredCoroutineAwaiter awaiter,
        CoroutineMethodBuilder<T>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<T>>> stateMachineBox)
    {
        _awaiter = awaiter;
        _stateMachineBox = stateMachineBox;
        _result = default!;
    }

    public readonly void AwaitUnsafeOnCompleted() => _awaiter.UnsafeOnCompleted(_stateMachineBox.MoveNextAction);

    public readonly void SetException(Exception e) => _stateMachineBox.SetException(e);

    public void GetResult() => _result = _awaiter.GetResult();

    public readonly void SetResult() => _stateMachineBox.SetResult(_result);
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
