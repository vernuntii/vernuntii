namespace Vernuntii.Coroutines.Iterators;

internal struct SuspensionPoint
{
    public static readonly SuspensionPoint AwaiterCompletionNotifierRequired = new() { _state = SuspensionPointState.AwaiterCompletionNotifierRequired };
    public static readonly SuspensionPoint None = new();

    internal SuspensionPointState _state;
    internal ICallableArgument? _argument;
    internal Key _argumentKey;
    internal ICoroutineCompletionSource? _argumentCompletionSource;
    internal ValueTask _awaiterCompletionNotifier;
    internal IRelativeCoroutineAwaiter _coroutineAwaiter;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowIfNotRequiringAwaiterCompletionNotifier(in SuspensionPoint operation)
    {
        if ((operation._state & SuspensionPointState.AwaiterCompletionNotifierRequired) == 0) {
            throw new InvalidOperationException($"The currnet operation state {operation._state} differs from the expected state {SuspensionPointState.AwaiterCompletionNotifierRequired}");
        }
    }

    internal void SupplyArgument(Key argumentKey, ICallableArgument argument, ICoroutineCompletionSource argumentCompletionSource)
    {
        ThrowIfNotRequiringAwaiterCompletionNotifier(in this);
        _state = SuspensionPointState.ArgumentSupplied | (_state & SuspensionPointState.AwaiterCompletionNotifierRequired);
        _argumentKey = argumentKey;
        _argument = argument;
        _argumentCompletionSource = argumentCompletionSource;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BeginSupplyingAwaiterCompletionNotifier(out ManualResetCoroutineCompletionSource<Nothing> externTaskCompletionNotifierSource)
    {
        ThrowIfNotRequiringAwaiterCompletionNotifier(in this);
        _state = SuspensionPointState.AwaiterCompletionNotifierSupplied | (_state & ~SuspensionPointState.AwaiterCompletionNotifierRequired);
        externTaskCompletionNotifierSource = ManualResetCoroutineCompletionSource<Nothing>.RentFromCache();
        _awaiterCompletionNotifier = externTaskCompletionNotifierSource.CreateValueTask();
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

    internal void SupplyCoroutineAwaiter(IRelativeCoroutineAwaiter coroutineAwaiter)
    {
        _state = SuspensionPointState.CoroutineAwaiterSupplied | _state;
        _coroutineAwaiter = coroutineAwaiter;
    }

    internal void RequireAwaiterCompletionNotifier() => this = AwaiterCompletionNotifierRequired;

    internal void ResetToNone() => this = None;
}
