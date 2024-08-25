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

    internal void ReceiveCallbackArgument<TArgument, TArgumentKey>(in TArgument argument, in TArgumentKey argumentKey)
        where TArgument : ICallbackArgument
        where TArgumentKey : IKey
    {
        if (_context.MustSupplyAsyncIterator()) {
            var asyncIteratorContextService = _context.GetAsyncIteratorContextService();
            asyncIteratorContextService.SupplyArgument(argument, argumentKey);
        } else {
            argument.Callback(ref _context);
        }
    }
}
