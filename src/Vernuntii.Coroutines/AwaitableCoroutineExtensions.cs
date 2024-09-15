using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class AwaitableCoroutineExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MarkCoroutineAsActedOn<TCoroutine>(this ref TCoroutine coroutine) where TCoroutine : struct, IRelativeCoroutine => coroutine.MarkCoroutineAsActedOn();
}
