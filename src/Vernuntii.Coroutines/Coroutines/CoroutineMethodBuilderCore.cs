using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vernuntii.Coroutines.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineMethodBuilderCore
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ActOnCoroutine<TCoroutine, TCoroutineActor>(
        ref TCoroutine coroutine,
        ref TCoroutineActor coroutineActor)
        where TCoroutine : IRelativeCoroutine
        where TCoroutineActor : ICoroutineActor
    {
        if (coroutine.IsChildCoroutine) {
            coroutineActor.ActOnChildCoroutine(ref coroutine);
        } else if (coroutine.IsSiblingCoroutine) {
            coroutineActor.ActOnSiblingCoroutine(ref coroutine);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void ActOnAwaiterIfCoroutine<TAwaiter, TCoroutineActor>(
        ref TAwaiter awaiter,
        ref TCoroutineActor coroutineActor)
        where TCoroutineActor : ICoroutineActor
    {
        if (null != default(TAwaiter) && awaiter is IRelativeCoroutineAwaiter) {
            ref var coroutineAwaiter = ref Unsafe.As<TAwaiter, CoroutineAwaiter>(ref awaiter);
            ActOnCoroutine(ref coroutineAwaiter, ref coroutineActor);
        }
    }

    internal static Coroutine MakeChildCoroutine(ref Coroutine nonChildCoroutine, ref CoroutineContext contextToBequest)
    {
        Debug.Assert(contextToBequest.BequesterOrigin == CoroutineContextBequesterOrigin.ChildCoroutine);
        var coroutineAwaiter = nonChildCoroutine.ConfigureAwait(false).GetAwaiter();
        var stateMachineBox = CoroutineMethodBuilder<Nothing>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder>>.RentFromCache();
        var coroutineBuilder = new CoroutineAwaiterMethodBuilder(in coroutineAwaiter, stateMachineBox);
        var stateMachine = new CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder>(coroutineBuilder) {
            State = -1
        };
        stateMachineBox.StateMachine = stateMachine;

        // We cannot assign the context directly to the state machine box,
        // because we do not want to implictly bequest the async iterator
        ref var stateMachineBoxCoroutineContext = ref stateMachineBox._coroutineContext;
        stateMachineBoxCoroutineContext.InheritContext(in contextToBequest);

        contextToBequest.SetResultStateMachine(stateMachineBox);
        stateMachineBox.MoveNext();
        return new Coroutine(new ValueTask(stateMachineBox, stateMachineBox.Version));
    }

    internal static Coroutine<T> MakeChildCoroutine<T>(ref Coroutine<T> nonChildCoroutine, ref CoroutineContext contextToBequest)
    {
        Debug.Assert(contextToBequest.BequesterOrigin == CoroutineContextBequesterOrigin.ChildCoroutine);
        var coroutineAwaiter = nonChildCoroutine.ConfigureAwait(false).GetAwaiter();
        var stateMachineBox = CoroutineMethodBuilder<T>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<T>>>.RentFromCache();
        var coroutineBuilder = new CoroutineAwaiterMethodBuilder<T>(in coroutineAwaiter, stateMachineBox);
        var stateMachine = new CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<T>>(coroutineBuilder) {
            State = -1
        };
        stateMachineBox.StateMachine = stateMachine;

        // We cannot assign the context directly to the state machine box,
        // because we do not want to implictly bequest the async iterator
        ref var stateMachineBoxCoroutineContext = ref stateMachineBox._coroutineContext;
        stateMachineBoxCoroutineContext.InheritContext(in contextToBequest);

        contextToBequest.SetResultStateMachine(stateMachineBox);
        stateMachineBox.MoveNext();
        return new Coroutine<T>(new ValueTask<T>(stateMachineBox, stateMachineBox.Version));
    }
}
