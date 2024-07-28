using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class AsyncCoroutineMethodBuilderCore
{
    internal static bool IsFailingToHandleCoroutineInvocation<TCoroutineAwaiter>(
        ref TCoroutineAwaiter coroutineAwaiter,
        in CoroutineScope coroutineScope) where TCoroutineAwaiter : ICoroutineInvocationAwaiter
    {
        if (coroutineAwaiter.IsChildCoroutine) {
            return true;
        }

        var coroutineInvocationAwaiter = Unsafe.As<TCoroutineAwaiter, CoroutineInvocation.CoroutineInvocationAwaiter>(ref coroutineAwaiter);

        if (coroutineInvocationAwaiter.ArgumentReceiverAcceptor is not null) {
            coroutineScope.HandleCoroutineInvocation(coroutineInvocationAwaiter.ArgumentReceiverAcceptor);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void ProcessAwaiterBeforeAwaitingOnCompleted<TAwaiter>(ref TAwaiter awaiter, in CoroutineScope coroutineScope)
    {
        if (default(TAwaiter) != null && awaiter is ICoroutineInvocationAwaiter) {
            var coroutineAwaiter = Unsafe.As<TAwaiter, Coroutine.CoroutineAwaiter>(ref awaiter);
            if (IsFailingToHandleCoroutineInvocation(ref coroutineAwaiter, coroutineScope)) {
                coroutineAwaiter.PropagateCoroutineScope(coroutineScope);
                coroutineAwaiter.StartStateMachine();
            }
        }
    }
}
