using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines.Iterators;

internal struct AsyncIteratorContextServiceOperation
{
    public static readonly AsyncIteratorContextServiceOperation AwaiterCompletionNotifierRequired = new() { State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired };
    public static readonly AsyncIteratorContextServiceOperation Unitialized = default;

    internal AsyncIteratorContextServiceOperationState State { get; private set; }
    internal ICallableArgument? Argument { get; private set; }
    internal Key ArgumentKey { get; private set; }
    internal IYieldReturnCompletionSource? ArgumentCompletionSource { get; private set; }
    internal ValueTask AwaiterCompletionNotifier { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowIfNotRequiringAwaiterCompletionNotifier(in AsyncIteratorContextServiceOperation operation)
    {
        if ((operation.State & AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired) == 0) {
            throw new InvalidOperationException($"The currnet operation state {operation.State} differs from the expected state {AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired}");
        }
    }

    internal void SupplyArgument(Key argumentKey, ICallableArgument argument, IYieldReturnCompletionSource argumentCompletionSource)
    {
        ThrowIfNotRequiringAwaiterCompletionNotifier(in this);
        State = AsyncIteratorContextServiceOperationState.ArgumentSupplied | (State & AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired);
        ArgumentKey = argumentKey;
        Argument = argument;
        ArgumentCompletionSource = argumentCompletionSource;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BeginSupplyingAwaiterCompletionNotifier(out ManualResetValueTaskCompletionSource<Nothing> externTaskCompletionNotifierSource)
    {
        ThrowIfNotRequiringAwaiterCompletionNotifier(in this);
        State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied | (State & ~AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired);
        externTaskCompletionNotifierSource = ManualResetValueTaskCompletionSource<Nothing>.RentFromCache();
        AwaiterCompletionNotifier = externTaskCompletionNotifierSource.CreateValueTask();
    }

    internal void SupplyAwaiterCompletionNotifier<TAwaiter>(ref TAwaiter awaiter) where TAwaiter : INotifyCompletion
    {
        BeginSupplyingAwaiterCompletionNotifier(out var externTaskCompletionNotifierSource); // and end with ..        
        awaiter.OnCompleted(externTaskCompletionNotifierSource.SetDefaultResult);
    }

    internal void SupplyAwaiterCriticalCompletionNotifier<TAwaiter>(ref TAwaiter awaiter) where TAwaiter : ICriticalNotifyCompletion
    {
        BeginSupplyingAwaiterCompletionNotifier(out var externTaskCompletionNotifierSource); // and end with ..
        awaiter.UnsafeOnCompleted(externTaskCompletionNotifierSource.SetDefaultResult);
    }

    internal void RequireAwaiterCompletionNotifier() => this = AwaiterCompletionNotifierRequired;

    internal void Uninitialize() => this = Unitialized;
}
