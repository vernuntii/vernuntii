using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineArgumentReceiverDelegateClosure
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Tuple<T1, T2, T3, T4, CoroutineArgumentReceiverDelegateWithClosure<T1, T2, T3, T4>> Create<T1, T2, T3, T4>(
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        CoroutineArgumentReceiverDelegateWithClosure<T1, T2, T3, T4> argumentReceiver) =>
        new(value1, value2, value3, value4, argumentReceiver);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Tuple<T1, T2, T3, T4, T5, CoroutineArgumentReceiverDelegateWithClosure<T1, T2, T3, T4, T5>> Create<T1, T2, T3, T4, T5>(
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        CoroutineArgumentReceiverDelegateWithClosure<T1, T2, T3, T4, T5> argumentReceiver) =>
        new(value1, value2, value3, value4, value5, argumentReceiver);
}
