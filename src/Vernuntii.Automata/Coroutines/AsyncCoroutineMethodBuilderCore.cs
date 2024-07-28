using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class AsyncCoroutineMethodBuilderCore
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void StartChildCoroutine<TAwaiter>(ref TAwaiter awaiter, in int argument)
    {
        if (default(TAwaiter) != null && awaiter is ICoroutineAwaiter) {
            var coroutineAwaiter = Unsafe.As<TAwaiter, Coroutine.CoroutineAwaiter>(ref awaiter);
            coroutineAwaiter.PropagateCoroutineArgument(argument);
            coroutineAwaiter.StartStateMachine();
        }
    }
}
