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

    internal void ReceiveCallableArgument<TArgument>(in Key argumentKey, in TArgument argument, IYieldCompletionSource completionSource)
        where TArgument : ICallableArgument
    {
        if (_context._isCoroutineAsyncIteratorSupplier) {
            var iteratorContextService = _preKnownAsyncIteratorContextService ?? _context.GetAsyncIteratorContextService();
            iteratorContextService._currentSuspensionPoint.SupplyArgument(argumentKey, argument, completionSource);
        } else {
            argument.Callback(in _context, completionSource);
        }
    }
}
