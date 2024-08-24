using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineEqualityComparer
{
    internal static bool Equals<TCoroutine>(in TCoroutine x, in TCoroutine y)
        where TCoroutine : IAwaiterAwareCoroutine
    {
        ref var typedX = ref Unsafe.As<TCoroutine, Coroutine>(ref Unsafe.AsRef(in x));
        ref var typedY = ref Unsafe.As<TCoroutine, Coroutine>(ref Unsafe.AsRef(in y));

        return typedX._task == typedY._task &&
            typedX._builder == typedY._builder &&
            ReferenceEquals(typedX._argumentReceiverDelegate, typedY._argumentReceiverDelegate);
    }
}
