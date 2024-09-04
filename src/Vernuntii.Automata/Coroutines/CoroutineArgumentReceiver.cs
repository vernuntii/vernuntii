using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

public delegate void CoroutineArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver);
public delegate void CoroutineArgumentReceiverDelegate<T1, T2, T3, T4>(Tuple<T1, T2, T3, T4> closure, ref CoroutineArgumentReceiver argumentReceiver);
public delegate void CoroutineArgumentReceiverDelegate<T1, T2, T3, T4, T5>(Tuple<T1, T2, T3, T4, T5> closure, ref CoroutineArgumentReceiver argumentReceiver);
public delegate void CoroutineArgumentReceiverDelegateWithClosure<T1, T2, T3, T4>(Tuple<T1, T2, T3, T4, CoroutineArgumentReceiverDelegateWithClosure<T1, T2, T3, T4>> closure, ref CoroutineArgumentReceiver argumentReceiver);
public delegate void CoroutineArgumentReceiverDelegateWithClosure<T1, T2, T3, T4, T5>(Tuple<T1, T2, T3, T4, T5, CoroutineArgumentReceiverDelegateWithClosure<T1, T2, T3, T4, T5>> closure, ref CoroutineArgumentReceiver argumentReceiver);

public ref struct CoroutineArgumentReceiver
{
    private ref CoroutineContext _context;

    internal CoroutineArgumentReceiver(ref CoroutineContext coroutineContext)
    {
        _context = ref coroutineContext;
    }

    internal void ReceiveCallableArgument<TArgument, TArgumentKey>(in TArgumentKey argumentKey, in TArgument argument, IYieldReturnCompletionSource completionSource)
        where TArgument : ICallableArgument
        where TArgumentKey : IKey
    {
        if (_context.IsAsyncIteratorSupplier) {
            var iteratorContextService = _context.GetAsyncIteratorContextService();
            iteratorContextService.CurrentOperation.SupplyArgument(argumentKey, argument, completionSource);
        } else {
            argument.Callback(in _context);
        }
    }
}
