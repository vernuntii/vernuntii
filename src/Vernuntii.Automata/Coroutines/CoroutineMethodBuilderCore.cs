using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineMethodBuilderCore
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void HandleCoroutine<TCoroutine, TCoroutineHandler>(
        ref TCoroutine coroutine,
        ref TCoroutineHandler coroutineHandler)
        where TCoroutine : IChildCoroutine, ISiblingCoroutine
        where TCoroutineHandler : ICoroutineHandler
    {
        if (coroutine.IsChildCoroutine) {
            coroutineHandler.HandleChildCoroutine(ref coroutine);
        } else if (coroutine.IsSiblingCoroutine) {
            coroutineHandler.HandleSiblingCoroutine(ref coroutine);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void AttemptHandlingCoroutineAwaiter<TAwaiter, TCoroutineHandler>(
    ref TAwaiter awaiter,
    ref TCoroutineHandler coroutineHandler)
    where TCoroutineHandler : ICoroutineHandler
    {
        if (null != default(TAwaiter) && awaiter is ICoroutineAwaiter) {
            ref var coroutineAwaiter = ref Unsafe.As<TAwaiter, Coroutine.CoroutineAwaiter>(ref awaiter);
            HandleCoroutine(ref coroutineAwaiter, ref coroutineHandler);
        }
    }
}
