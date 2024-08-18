using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineEqualityComparer
{
    internal unsafe static bool Equal<TCoroutine>(in TCoroutine x, in TCoroutine y)
        where TCoroutine : IEntryCoroutine
    {
        ref var typedX = ref Unsafe.As<TCoroutine, Coroutine>(ref Unsafe.AsRef(x));
        ref var typedY = ref Unsafe.As<TCoroutine, Coroutine>(ref Unsafe.AsRef(y));

        return typedX._task.Equals(typedY._task) &&
            typedX._builder == typedY._builder &&
            ReferenceEquals(typedX._argumentReceiverDelegate, typedY._argumentReceiverDelegate);
    }
}
