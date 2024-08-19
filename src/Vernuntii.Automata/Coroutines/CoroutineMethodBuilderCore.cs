using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineMethodBuilderCore
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void HandleCoroutine<TCoroutine, TCoroutineHandler>(
        ref TCoroutine coroutine,
        ref TCoroutineHandler coroutineHandler)
        where TCoroutine : ICoroutine
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

    internal static Coroutine MakeChildCoroutine(ref Coroutine coroutine, ref CoroutineStackNode coroutineNode)
    {
        var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
        var stateMachineBox = CoroutineMethodBuilder<VoidCoroutineResult>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder>>.RentFromCache();
        var coroutineBuilder = new CoroutineAwaiterMethodBuilder(in coroutineAwaiter, stateMachineBox);
        var stateMachine = new CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder>(coroutineBuilder);
        stateMachine.State = -1;
        stateMachineBox.StateMachine = stateMachine;
        stateMachineBox.MoveNext();
        coroutineNode.SetResultStateMachine(stateMachineBox);
        return new Coroutine(new ValueTask(stateMachineBox, stateMachineBox.Version));
    }

    internal static Coroutine<T> MakeChildCoroutine<T>(ref Coroutine<T> coroutine, ref CoroutineStackNode coroutineNode)
    {
        var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
        var stateMachineBox = CoroutineMethodBuilder<T>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<T>>>.RentFromCache();
        var coroutineBuilder = new CoroutineAwaiterMethodBuilder<T>(in coroutineAwaiter, stateMachineBox);
        var stateMachine = new CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<T>>(coroutineBuilder);
        stateMachine.State = -1;
        stateMachineBox.StateMachine = stateMachine;
        stateMachineBox.MoveNext();
        coroutineNode.SetResultStateMachine(stateMachineBox);
        return new Coroutine<T>(new ValueTask<T>(stateMachineBox, stateMachineBox.Version));
    }
}
