using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

public delegate void CoroutineArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver);

public readonly ref struct CoroutineArgumentReceiver
{
    internal readonly ref CoroutineContext _context;

    internal CoroutineArgumentReceiver(in CoroutineContext coroutineContext)
    {
        _context = ref Unsafe.AsRef(in coroutineContext);
    }

    internal void ReceiveCallableArgument<TArgument>(in Key argumentKey, in TArgument argument, IYieldCompletionSource completionSource)
        where TArgument : ICallableArgument
    {
        if (_context._isCoroutineAsyncIteratorSupplier) {
            var iteratorContextService = _context.GetAsyncIteratorContextService();
            iteratorContextService.CurrentOperation.SupplyArgument(argumentKey, argument, completionSource);
        } else {
            argument.Callback(in _context, completionSource);
        }
    }
}
