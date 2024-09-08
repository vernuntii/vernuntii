using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class AwaitableCoroutineExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MarkCoroutineAsHandled<TCoroutine>(this ref TCoroutine coroutine) where TCoroutine : struct, IAwaitableCoroutine =>
        coroutine.MarkCoroutineAsHandled();
}
