using System.Runtime.CompilerServices;
using InlineIL;
using static InlineIL.IL.Emit;

namespace Vernuntii.Coroutines;

internal static class CoroutineMethodBuilderCore
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

    //[MethodImpl(MethodImplOptions.NoInlining)]
    //internal static bool IsGenericCoroutineAwaiterCore<TCoroutineAwaiter>(
    //    ref TCoroutineAwaiter coroutineAwaiter)
    //    where TCoroutineAwaiter : struct, ICoroutineAwaiter
    //{
    //    return coroutineAwaiter.IsGenericCoroutine;
    //}

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //internal static bool IsGenericCoroutineAwaiter<TCoroutineAwaiter>(
    //    ref TCoroutineAwaiter coroutineAwaiter
    //    )
    //{
    //    IL.PushInRef(coroutineAwaiter);
    //    Call(new MethodRef(
    //            typeof(AsyncCoroutineMethodBuilderCore),
    //            nameof(IsGenericCoroutineAwaiterCore),
    //            genericParameterCount: 1,
    //            TypeRef.MethodGenericParameters[0].MakeByRefType())
    //        .MakeGenericMethod(typeof(Coroutine<object>.CoroutineAwaiter))
    //    );
    //    return IL.Return<bool>();
    //}

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void ProcessAwaiterBeforeAwaitingOnCompleted<TAwaiter, TCoroutineHandler>(
    ref TAwaiter awaiter,
    ref TCoroutineHandler coroutineHandler)
    where TCoroutineHandler : ICoroutineHandler
    {
        if (default(TAwaiter) != null && awaiter is ICoroutineAwaiter) {
            //if (IsGenericCoroutineAwaiter(ref awaiter)) {
                ref var coroutineAwaiter = ref Unsafe.As<TAwaiter, Coroutine.CoroutineAwaiter>(ref awaiter);
                if (IsFailingToHandleInlineCoroutine(ref coroutineAwaiter, ref coroutineHandler)) {
                    coroutineHandler.HandleChildCoroutine(ref coroutineAwaiter);
            }
            //} else {
            //    ref var coroutineAwaiter = ref Unsafe.As<TAwaiter, Coroutine.CoroutineAwaiter>(ref awaiter);
            //    if (IsFailingToHandleInlineCoroutine(ref coroutineAwaiter, ref coroutineHandler)) {
            //        coroutineHandler.HandleChildCoroutine(ref coroutineAwaiter);
            //    }
            //}
        }
    }
}
