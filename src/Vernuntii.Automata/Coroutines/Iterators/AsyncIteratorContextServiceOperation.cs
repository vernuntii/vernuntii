namespace Vernuntii.Coroutines.Iterators;

internal record AsyncIteratorContextServiceOperation
{
    public static readonly AsyncIteratorContextServiceOperation AwaiterCompletionNotifierRequired = new AsyncIteratorContextServiceOperation() { State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired };

    internal ICallableArgument? Argument { get; init; }
    internal IKey? ArgumentKey { get; init; }
    internal IYieldReturnCompletionSource? ArgumentCompletionSource { get; init; }
    internal ValueTask AwaiterCompletionNotifier { get; init; }
    internal AsyncIteratorContextServiceOperationState State { get; init; }
}
