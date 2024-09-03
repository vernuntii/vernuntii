using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines.Iterators;

internal struct AsyncIteratorContextServiceOperation
{
    public static readonly AsyncIteratorContextServiceOperation AwaiterCompletionNotifierRequired = new AsyncIteratorContextServiceOperation() { State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired };

    internal ICallableArgument? Argument { get; private set; }
    internal IKey? ArgumentKey { get; private set; }
    internal IYieldReturnCompletionSource? ArgumentCompletionSource { get; private set; }
    internal ValueTask AwaiterCompletionNotifier { get; private set; }
    internal AsyncIteratorContextServiceOperationState State { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowIfNotRequiringAwaiterCompletionNotifier(in AsyncIteratorContextServiceOperation operation)
    {
        if ((operation.State & AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired) == 0) {
            throw new InvalidOperationException($"The currnet operation state {operation.State} differs from the expected state {AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired}");
        }
    }

    internal void SupplyArgument(IKey argumentKey, ICallableArgument argument, IYieldReturnCompletionSource argumentCompletionSource)
    {
        ThrowIfNotRequiringAwaiterCompletionNotifier(in this);
        State = AsyncIteratorContextServiceOperationState.ArgumentSupplied | (State & AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired);
        ArgumentKey = argumentKey;
        Argument = argument;
        ArgumentCompletionSource = argumentCompletionSource;
    }

    internal void SupplyAwaiterCompletionNotifier<TAwaiter>(ref TAwaiter awaiter) where TAwaiter : INotifyCompletion
    {
        ThrowIfNotRequiringAwaiterCompletionNotifier(in this);
        var externTaskCompletionNotifierSource = ValueTaskCompletionSource<Nothing>.RentFromCache();
        State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied | (State & ~AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired);
        AwaiterCompletionNotifier = externTaskCompletionNotifierSource.CreateValueTask();
        awaiter.OnCompleted(externTaskCompletionNotifierSource.SetDefaultResult);
    }

    internal void SupplyAwaiterCriticalCompletionNotifier<TAwaiter>(ref TAwaiter awaiter) where TAwaiter : ICriticalNotifyCompletion
    {
        ThrowIfNotRequiringAwaiterCompletionNotifier(in this);
        var externTaskCompletionNotifierSource = ValueTaskCompletionSource<Nothing>.RentFromCache();
        State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied | (State & ~AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired);
        AwaiterCompletionNotifier = externTaskCompletionNotifierSource.CreateValueTask();
        awaiter.UnsafeOnCompleted(externTaskCompletionNotifierSource.SetDefaultResult);
    }

    internal void RequireAwaiterCompletionNotifier()
    {
        State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired;
        ArgumentKey = default;
        Argument = default;
        ArgumentCompletionSource = default;
        AwaiterCompletionNotifier = default;
    }

    internal void Uninitialize() {
        State = 0;
        ArgumentKey = default;
        Argument = default;
        ArgumentCompletionSource = default;
        AwaiterCompletionNotifier = default;
    }
}
