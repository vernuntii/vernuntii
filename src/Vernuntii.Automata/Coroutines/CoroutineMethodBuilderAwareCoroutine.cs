using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineMethodBuilderAwareCoroutine
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartOrphanCoroutine<TCoroutine>(this ref TCoroutine coroutine) where TCoroutine : struct, ICoroutineMethodBuilderAwareCoroutine
    {
        var context = new CoroutineContext();
        var node = new CoroutineStackNode(context);
        coroutine.PropagateCoroutineNode(ref node);
        coroutine.StartStateMachine();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartChildCoroutine<TCoroutine>(this ref TCoroutine coroutine, ref CoroutineStackNode coroutineNode) where TCoroutine : struct, ICoroutineMethodBuilderAwareCoroutine
    {
        coroutine.PropagateCoroutineNode(ref coroutineNode);
        coroutine.StartStateMachine();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MarkCoroutineAsHandled<TCoroutine>(this ref TCoroutine coroutine) where TCoroutine : struct, ICoroutineMethodBuilderAwareCoroutine
    {
        coroutine.MarkCoroutineAsHandled();
    }
}
