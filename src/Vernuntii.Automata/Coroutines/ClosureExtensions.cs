using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class ClosureExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Delegate<T1>(this Tuple<T1, Action<T1>> closure) => closure.Item2(closure.Item1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Delegate<T1, T2>(this Tuple<T1, T2, Action<T1, T2>> closure) => closure.Item3(closure.Item1, closure.Item2);
}
