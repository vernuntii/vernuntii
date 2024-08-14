using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineMethodBuilderAwareCoroutine
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MarkCoroutineAsHandled<TCoroutine>(this ref TCoroutine coroutine) where TCoroutine : struct, IRootCoroutine
    {
        coroutine.MarkCoroutineAsHandled();
    }
}
