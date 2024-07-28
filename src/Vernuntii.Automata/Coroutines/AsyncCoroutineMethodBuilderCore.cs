using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class AsyncCoroutineMethodBuilderCore
{
    public static bool IsBoxed<T>(ref T value)
    {
        return
            (typeof(T).IsInterface || typeof(T) == typeof(object)) &&
            value != null &&
            value.GetType().IsValueType;
    }

    internal static bool IsFailingToHandleCoroutineInvocation<TCoroutineAwaiter>(
        ref TCoroutineAwaiter coroutineAwaiter,
        in int argument) where TCoroutineAwaiter : ICoroutineInvocationAwaiter
    {
        if (coroutineAwaiter.IsChildCoroutine) {
            return true;
        }

        var coroutineInvocationAwaiter = Unsafe.As<TCoroutineAwaiter, CoroutineInvocation.CoroutineInvocationAwaiter>(ref coroutineAwaiter);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void ProcessAwaiterBeforeAwaitingOnCompleted<TAwaiter>(ref TAwaiter awaiter, in int argument)
    {
        if (default(TAwaiter) != null && awaiter is ICoroutineInvocationAwaiter) {
            var coroutineAwaiter = Unsafe.As<TAwaiter, Coroutine.CoroutineAwaiter>(ref awaiter);
            if (IsFailingToHandleCoroutineInvocation(ref coroutineAwaiter, argument)) {
                coroutineAwaiter.PropagateCoroutineArgument(argument);
                coroutineAwaiter.StartStateMachine();
            }
        }
    }
}
