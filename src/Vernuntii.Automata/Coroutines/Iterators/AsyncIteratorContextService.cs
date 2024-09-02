using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines.Iterators;

internal class AsyncIteratorContextService(AsyncIteratorContextServiceOperation initialOperation)
{
    private AsyncIteratorContextServiceOperation _currentOperation = initialOperation;

    public AsyncIteratorContextServiceOperation CurrentOperation => _currentOperation;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowIfNotRequiringAwaiterCompletionNotifier(AsyncIteratorContextServiceOperation operation)
    {
        if ((operation.State & AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired) == 0) {
            throw new InvalidOperationException($"The currnet operation state {operation.State} differs from the expected state {AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired}");
        }
    }

    internal void SupplyArgument(IKey argumentKey, ICallableArgument argument, IYieldReturnCompletionSource argumentCompletionSource)
    {
        var operation = _currentOperation;
        ThrowIfNotRequiringAwaiterCompletionNotifier(operation);
        _currentOperation = operation with {
            State = AsyncIteratorContextServiceOperationState.ArgumentSupplied | (operation.State & AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired),
            ArgumentKey = argumentKey,
            Argument = argument,
            ArgumentCompletionSource = argumentCompletionSource
        };
    }

    internal void SupplyAwaiterCompletionNotifier<TAwaiter>(ref TAwaiter awaiter) where TAwaiter : INotifyCompletion
    {
        var operation = _currentOperation;
        ThrowIfNotRequiringAwaiterCompletionNotifier(operation);
        var externTaskCompletionNotifierSource = ValueTaskCompletionSource<Nothing>.RentFromCache();
        _currentOperation = operation with {
            State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied | (operation.State & ~AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired),
            AwaiterCompletionNotifier = externTaskCompletionNotifierSource.CreateValueTask(),
        };
        awaiter.OnCompleted(() => externTaskCompletionNotifierSource.SetDefaultResult());
    }

    internal void SupplyAwaiterCriticalCompletionNotifier<TAwaiter>(ref TAwaiter awaiter) where TAwaiter : ICriticalNotifyCompletion
    {
        var operation = _currentOperation;
        ThrowIfNotRequiringAwaiterCompletionNotifier(operation);
        var externTaskCompletionNotifierSource = ValueTaskCompletionSource<Nothing>.RentFromCache();
        _currentOperation = operation with {
            State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied | (operation.State & ~AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired),
            AwaiterCompletionNotifier = externTaskCompletionNotifierSource.CreateValueTask()
        };
        awaiter.UnsafeOnCompleted(() => externTaskCompletionNotifierSource.SetDefaultResult());
    }

    internal void RequireAwaiterCompletionNotifier()
    {
        var operation = _currentOperation;
        _currentOperation = new AsyncIteratorContextServiceOperation() {
            State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired
        };
    }
}
