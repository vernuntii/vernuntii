using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class ActionClosure
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Tuple<T1, Action<T1>> Create<T1>(T1 value1, Action<T1> value2) => new(value1, value2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Tuple<T1, T2, Action<T1, T2>> Create<T1, T2>(T1 value1, T2 value2, Action<T1, T2> value3) => new(value1, value2, value3);
}
