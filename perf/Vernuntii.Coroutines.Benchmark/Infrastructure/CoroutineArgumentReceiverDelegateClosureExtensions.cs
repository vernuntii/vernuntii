using System;

namespace Vernuntii.Coroutines.Benchmark.Infrastructure;

public static class CoroutineArgumentReceiverDelegateClosureExtensions
{
    public static void CoroutineArgumentReceiver<T1, T2, T3, T4>(this Tuple<T1, T2, T3, T4, CoroutineArgumentReceiverDelegateWithClosure<T1, T2, T3, T4>> closure, ref CoroutineArgumentReceiver argumentReceiver)
    {
        closure.Item5(closure, ref argumentReceiver);
    }

    public static void CoroutineArgumentReceiver<T1, T2, T3, T4, T5>(this Tuple<T1, T2, T3, T4, T5, CoroutineArgumentReceiverDelegateWithClosure<T1, T2, T3, T4, T5>> closure, ref CoroutineArgumentReceiver argumentReceiver)
    {
        closure.Item6(closure, ref argumentReceiver);
    }
}
