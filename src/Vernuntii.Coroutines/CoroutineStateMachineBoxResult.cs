namespace Vernuntii.Coroutines;

internal class CoroutineStateMachineBoxResult<TResult> : IEquatable<CoroutineStateMachineBoxResult<TResult>>
{
    public static readonly CoroutineStateMachineBoxResult<TResult> Default = new();

    private CoroutineStateMachineBoxResult()
    {
        Result = default!;
    }

    public CoroutineStateMachineBoxResult(int forkCount)
    {
        Result = default!;
        ForkCount = forkCount;
    }

    public CoroutineStateMachineBoxResult(int forkCount, TResult result)
    {
        ForkCount = forkCount;
        State = ResultState.HasResult;
        Result = result;
    }

    public CoroutineStateMachineBoxResult(int forkCount, Exception error)
    {
        ForkCount = forkCount;
        State = ResultState.HasError;
        Result = default!;
        Error = error;
    }

    public CoroutineStateMachineBoxResult(CoroutineStateMachineBoxResult<TResult> original, int forkCount)
    {
        ForkCount = forkCount;
        State = original.State;
        Result = original.Result;
        Error = original.Error;
    }

    public int ForkCount { get; init; }

    [MemberNotNullWhen(true, nameof(Result))]
    public bool HasResult {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            return State.HasFlag(ResultState.HasResult);
        }
    }

    [MemberNotNullWhen(true, nameof(Error))]
    public bool HasError {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            return State.HasFlag(ResultState.HasError);
        }
    }

    public ResultState State { get; init; }
    public TResult Result { get; init; }
    public Exception? Error { get; init; }

    public bool Equals(CoroutineStateMachineBoxResult<TResult>? other)
    {
        if (other is null) {
            return false;
        }

        return ForkCount == other.ForkCount &&
            State == other.State;
    }

    internal enum ResultState : byte
    {
        NotYetComputed = 0,
        HasResult = 1,
        HasError = 2
    }
}
