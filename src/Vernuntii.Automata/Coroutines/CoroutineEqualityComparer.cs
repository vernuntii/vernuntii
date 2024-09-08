using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineEqualityComparer
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Equals(in Coroutine x, in Coroutine y)
    {
        return x._builder == y._builder &&
            ReferenceEquals(x._argumentReceiverDelegate, y._argumentReceiverDelegate) &&
            x._task == y._task;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Equals<TResult>(in Coroutine<TResult> x, in Coroutine<TResult> y)
    {
        return x._builder == y._builder &&
            ReferenceEquals(x._argumentReceiverDelegate, y._argumentReceiverDelegate) &&
            x._task == y._task;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetHashCode(in Coroutine co)
    {
        var hashCode = new HashCode();
        hashCode.Add(co._builder);
        hashCode.Add(co._argumentReceiverDelegate);
        hashCode.Add(co._task);
        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetHashCode<TResult>(in Coroutine<TResult> co)
    {
        var hashCode = new HashCode();
        hashCode.Add(co._builder);
        hashCode.Add(co._argumentReceiverDelegate);
        hashCode.Add(co._task);
        return hashCode.ToHashCode();
    }
}
