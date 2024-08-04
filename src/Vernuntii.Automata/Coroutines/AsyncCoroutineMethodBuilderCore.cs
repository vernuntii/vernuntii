using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class AsyncCoroutineMethodBuilderCore
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsFailingToHandleInlineCoroutine<TCoroutineAwaiter, TCoroutineHandler>(
        ref TCoroutineAwaiter coroutineAwaiter,
        ref TCoroutineHandler coroutineHandler)
        where TCoroutineAwaiter : ICoroutineAwaiter
        where TCoroutineHandler : ICoroutineHandler
    {
        if (coroutineAwaiter.IsChildCoroutine) {
            return true;
        }

        if (coroutineAwaiter.ArgumentReceiverDelegate is not null) {
            coroutineHandler.HandleDirectCoroutine(coroutineAwaiter.ArgumentReceiverDelegate);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void ProcessAwaiterBeforeAwaitingOnCompleted<TAwaiter, TCoroutineHandler>(
        ref TAwaiter awaiter,
        ref TCoroutineHandler coroutineHandler)
        where TCoroutineHandler : ICoroutineHandler
    {
        if (default(TAwaiter) != null && awaiter is ICoroutineAwaiter) {
            ref var coroutineAwaiter = ref Unsafe.As<TAwaiter, Coroutine.CoroutineAwaiter>(ref awaiter);
            if (IsFailingToHandleInlineCoroutine(ref coroutineAwaiter, ref coroutineHandler)) {
                coroutineHandler.HandleChildCoroutine(ref coroutineAwaiter);
            }
        }
    }
}
