using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vernuntii.Coroutines.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineMethodBuilderCore
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ActOnCoroutine<TCoroutine>(
        ref TCoroutine coroutine,
        ref CoroutineContext context)
        where TCoroutine : IRelativeCoroutine
    {
        switch (coroutine.CoroutineAction) {
            case CoroutineAction.Task:
                return;
            case CoroutineAction.Sibling:
                Debug.Assert(coroutine.CoroutineActioner is not null);
                var siblingCoroutine = Unsafe.As<ISiblingCoroutine>(coroutine.CoroutineActioner);
                var argumentReceiver = new CoroutineArgumentReceiver(ref context);
                siblingCoroutine.AcceptCoroutineArgumentReceiver(ref argumentReceiver);
                break;
            case CoroutineAction.Child:
                Debug.Assert(coroutine.CoroutineActioner is not null);
                var childCoroutine = Unsafe.As<IChildCoroutine>(coroutine.CoroutineActioner);
                childCoroutine.StartCoroutine(in context);
                break;
            default:
                throw new Exception();
        }

        coroutine.MarkCoroutineAsActedOn();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void ActOnAwaiterIfCoroutine<TAwaiter>(
        ref TAwaiter awaiter,
        ref CoroutineContext context)
    {
        if (null != default(TAwaiter) && awaiter is IRelativeCoroutine) {
            ref var coroutineAwaiter = ref Unsafe.As<TAwaiter, CoroutineAwaiter>(ref awaiter);
            ActOnCoroutine(ref coroutineAwaiter, ref context);
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
