using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class AsyncCoroutineMethodBuilderCore
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsFailingToHandleInlineCoroutine<TCoroutineAwaiter>(
        ref TCoroutineAwaiter coroutineAwaiter,
        ref CoroutineStackNode coroutineNode) where TCoroutineAwaiter : ICoroutineAwaiter
    {
        if (coroutineAwaiter.IsChildCoroutine) {
            return true;
        }

        if (coroutineAwaiter.ArgumentReceiverAcceptor is not null) {
            coroutineNode.HandleInlineCoroutine(coroutineAwaiter.ArgumentReceiverAcceptor);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void ProcessAwaiterBeforeAwaitingOnCompleted<TAwaiter>(ref TAwaiter awaiter, ref CoroutineStackNode coroutineNode)
    {
        if (default(TAwaiter) != null && awaiter is ICoroutineAwaiter) {
            var coroutineAwaiter = Unsafe.As<TAwaiter, Coroutine.CoroutineAwaiter>(ref awaiter);
            if (IsFailingToHandleInlineCoroutine(ref coroutineAwaiter, ref coroutineNode)) {
                coroutineAwaiter.PropagateCoroutineNode(ref coroutineNode);
                coroutineAwaiter.StartStateMachine();
            }
        }
    }
}
