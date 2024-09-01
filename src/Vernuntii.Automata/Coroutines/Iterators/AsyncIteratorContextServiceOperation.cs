namespace Vernuntii.Coroutines.Iterators;

internal record AsyncIteratorContextServiceOperation
{
    public static readonly AsyncIteratorContextServiceOperation RequiringAwaiterCompletionNotifier = new AsyncIteratorContextServiceOperation() { State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired };

    internal ICallableArgument? Argument { get; init; }
    internal IKey? ArgumentKey { get; init; }
    internal IAsyncIterationCompletionSource? ArgumentCompletionSource { get; init; }
    internal ValueTask AwaiterCompletionNotifier { get; init; }
    internal AsyncIteratorContextServiceOperationState State { get; init; }
}
