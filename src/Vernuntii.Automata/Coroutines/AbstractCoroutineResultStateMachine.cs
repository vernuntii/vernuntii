using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal abstract class AbstractCoroutineResultStateMachine<T> : ICoroutineResultStateMachine
{
    State? _state;

    public AbstractCoroutineResultStateMachine()
    {
        _state = State.Default;
    }

    protected abstract void SetExceptionCore(Exception error);

    protected abstract void SetResultCore(T result);

    public void AwaitUnsafeOnCompleted<TAwaiter>(ref TAwaiter awaiter, Action continuation)
        where TAwaiter : ICriticalNotifyCompletion
    {
        State? currentState;
        State newState;

        do {
            currentState = _state;

            if (currentState is null || currentState.HasResult) {
                throw new InvalidOperationException("Result state machine has already finished");
            }

            newState = new State(currentState.ForkCount + 1);
        } while (Interlocked.CompareExchange(ref _state, newState, currentState)?.ForkCount != currentState.ForkCount);

        awaiter.UnsafeOnCompleted(() => {
            Exception? childError = null;

            try {
                continuation();
            } catch (Exception error) {
                childError = error;
            }

            State? currentState;
            State? newState;

            do {
                currentState = _state;

                if (currentState is null) {
                    return;
                }

                if (currentState.HasResult) {
                    if (currentState.ForkCount == 1) {
                        newState = null;
                    } else {
                        newState = new State(currentState.ForkCount - 1, currentState.HasResult, currentState.Result);
                    }
                } else {
                    if (currentState.ForkCount == 1) {
                        newState = State.Default;
                    } else {
                        newState = new State(currentState.ForkCount - 1);
                    }
                }
            } while (Interlocked.CompareExchange(ref _state, newState, currentState)?.ForkCount != currentState.ForkCount);

            if (newState is null) {
                if (currentState.HasError) {
                    SetExceptionCore(currentState.Error);
                } else if (childError is not null)
                    SetExceptionCore(childError);
                else {
                    SetResultCore(currentState.Result);
                }
            }
        });
    }

    public void SetException(Exception error)
    {
        State currentState;
        State? newState;

        do {
            currentState = _state!; // Cannot be null at this state
            newState = currentState.ForkCount == 0 ? null : new State(currentState.ForkCount, hasError: true, error);
        } while (Interlocked.CompareExchange(ref _state, newState, currentState)!.ForkCount != currentState.ForkCount);

        if (newState is null) {
            SetExceptionCore(error);
        }
    }

    public void SetResult(T result)
    {
        State currentState;
        State? newState;

        do {
            currentState = _state!; // Cannot be null at this state
            newState = currentState.ForkCount == 0 ? null : new State(currentState.ForkCount, hasResult: true, result);
        } while (Interlocked.CompareExchange(ref _state, newState, currentState)!.ForkCount != currentState.ForkCount);

        if (newState is null) {
            SetResultCore(result);
        }
    }

    protected class State : IEquatable<State>
    {
        internal readonly static State Default = new(forkCount: 0, hasResult: false, result: default!);

        public State(int forkCount)
        {
            ForkCount = forkCount;
            Result = default!;
        }

        public State(int forkCount, bool hasResult, T result)
        {
            ForkCount = forkCount;
            HasResult = hasResult;
            Result = result;
        }

        public State(int forkCount, bool hasError, Exception error)
        {
            ForkCount = forkCount;
            Result = default!;
            HasError = hasError;
            Error = error;
        }

        public int ForkCount { get; init; }
        [MemberNotNullWhen(true, nameof(Result))]
        public bool HasResult { get; init; }
        [MemberNotNullWhen(true, nameof(Error))]
        public bool HasError { get; init; }
        public T Result { get; init; }
        public Exception? Error { get; init; }

        public bool Equals(State? other)
        {
            if (other is null) {
                return false;
            }

            return ForkCount == other.ForkCount &&
                HasResult == other.HasResult;
        }
    }
}
