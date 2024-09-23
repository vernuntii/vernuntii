using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

public delegate void CoroutineArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver);

public readonly ref struct CoroutineArgumentReceiver
{
    internal readonly ref CoroutineContext _context;
    private readonly AsyncIteratorContextService? _preKnownAsyncIteratorContextService;

    internal CoroutineArgumentReceiver(in CoroutineContext coroutineContext, AsyncIteratorContextService? preKnownAsyncIteratorContextService = null)
    {
        _context = ref Unsafe.AsRef(in coroutineContext);
        _preKnownAsyncIteratorContextService = preKnownAsyncIteratorContextService;
    }

    public void ReceiveCallableArgument<TArgument, TCompletionSource>(in Key argumentKey, in TArgument argument, TCompletionSource completionSource)
        where TArgument : ICallableArgument<TCompletionSource>
        where TCompletionSource : class, ICoroutineCompletionSource
    {
        if (_context._isCoroutineAsyncIteratorSupplier) {
            var iteratorContextService = _preKnownAsyncIteratorContextService ?? _context.GetAsyncIteratorContextService();
            iteratorContextService._currentSuspensionPoint.SupplyArgument(argumentKey, argument, completionSource);
        } else {
            argument.Callback(in _context, completionSource);
        }
    }
}
