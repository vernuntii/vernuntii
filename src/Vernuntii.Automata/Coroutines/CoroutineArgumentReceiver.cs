using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

public delegate void CoroutineArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver);

public ref struct CoroutineArgumentReceiver
{
    private ref CoroutineContext _context;

    internal CoroutineArgumentReceiver(ref CoroutineContext coroutineContext)
    {
        _context = ref coroutineContext;
    }

    internal void ReceiveCallableArgument<TArgument, TArgumentKey>(in TArgumentKey argumentKey, in TArgument argument, IAsyncIterationCompletionSource completionSource)
        where TArgument : ICallableArgument
        where TArgumentKey : IKey
    {
        if (_context.MustSupplyAsyncIterator()) {
            var iteratorContextService = _context.GetAsyncIteratorContextService();
            iteratorContextService.SupplyArgument(argumentKey, argument, completionSource);
        } else {
            argument.Callback(in _context);
        }
    }
}
