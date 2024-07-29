using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class AsyncCoroutineMethodBuilderCore
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsFailingToHandleCoroutineInvocation<TCoroutineAwaiter>(
        ref TCoroutineAwaiter coroutineAwaiter,
        in CoroutineContext coroutineContext) where TCoroutineAwaiter : ICoroutineAwaiter
    {
        if (coroutineAwaiter.IsChildCoroutine) {
            return true;
        }

        if (coroutineAwaiter.ArgumentReceiverAcceptor is not null) {
            coroutineContext.HandleCoroutineInvocation(coroutineAwaiter.ArgumentReceiverAcceptor);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void ProcessAwaiterBeforeAwaitingOnCompleted<TAwaiter>(ref TAwaiter awaiter, in CoroutineContext coroutineContext)
    {
        if (default(TAwaiter) != null && awaiter is ICoroutineAwaiter) {
            var coroutineAwaiter = Unsafe.As<TAwaiter, Coroutine.CoroutineAwaiter>(ref awaiter);
            if (IsFailingToHandleCoroutineInvocation(ref coroutineAwaiter, coroutineContext)) {
                coroutineAwaiter.PropagateCoroutineContext(coroutineContext);
                coroutineAwaiter.StartStateMachine();
            }
        }
    }
}
