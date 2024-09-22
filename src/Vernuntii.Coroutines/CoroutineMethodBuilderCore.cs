using System.Diagnostics;
using Vernuntii.Coroutines.CompilerServices;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

internal static class CoroutineMethodBuilderCore
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ActOnCoroutine<TCoroutine>(
        ref TCoroutine coroutine,
        in CoroutineContext context,
        AsyncIteratorContextService? preKnownAsyncIteratorContextService = null)
        where TCoroutine : IRelativeCoroutine
    {
        switch (coroutine.CoroutineAction) {
            case CoroutineAction.None:
                return;
            case CoroutineAction.Sibling:
                Debug.Assert(coroutine.CoroutineActioner is not null);
                var siblingCoroutine = Unsafe.As<ISiblingCoroutine>(coroutine.CoroutineActioner);
                var argumentReceiver = new CoroutineArgumentReceiver(in context, preKnownAsyncIteratorContextService);
                siblingCoroutine.ActOnCoroutine(ref argumentReceiver);
                break;
            case CoroutineAction.Child:
                Debug.Assert(coroutine.CoroutineActioner is not null);
                var childCoroutine = Unsafe.As<IChildCoroutine>(coroutine.CoroutineActioner);
                childCoroutine.ActOnCoroutine(in context);
                break;
            default:
                throw new Exception();
        }

        coroutine.MarkCoroutineAsActedOn();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ActOnCoroutine<TCoroutine>(
        ref TCoroutine coroutine)
        where TCoroutine : IRelativeCoroutine
    {
        switch (coroutine.CoroutineAction) {
            case CoroutineAction.Sibling:
                Debug.Assert(coroutine.CoroutineActioner is not null);
                var siblingCoroutine = Unsafe.As<ISiblingCoroutine>(coroutine.CoroutineActioner);
                var argumentReceiver = new CoroutineArgumentReceiver(in CoroutineContext.s_statelessCoroutineContext);
                siblingCoroutine.ActOnCoroutine(ref argumentReceiver);
                break;
            case CoroutineAction.Child:
                Debug.Assert(coroutine.CoroutineActioner is not null);
                var childCoroutine = Unsafe.As<IChildCoroutine>(coroutine.CoroutineActioner);
                childCoroutine.ActOnCoroutine(in CoroutineContext.s_statelessCoroutineContext);
                break;
            default:
                throw new Exception();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static bool ActOnAwaiterIfCoroutineAwaiter<TAwaiter>(
        ref TAwaiter awaiter,
        ref CoroutineContext contextToBequest,
        AsyncIteratorContextService? preKnownAsyncIteratorContextService = null)
    {
        if (null != default(TAwaiter) && awaiter is IRelativeCoroutineAwaiter) {
            ref var coroutineAwaiter = ref Unsafe.As<TAwaiter, CoroutineAwaiter>(ref awaiter);
            ActOnCoroutine(ref coroutineAwaiter, in contextToBequest, preKnownAsyncIteratorContextService);
            return true;
        }

        return false;
    }

    internal static Coroutine MakeChildCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutineAwaiter, ref CoroutineContext contextToBequest)
        where TCoroutineAwaiter : struct, ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        Debug.Assert(contextToBequest.BequesterOrigin == CoroutineContextBequesterOrigin.ChildCoroutine);
        var stateMachineHolder = CoroutineStateMachineHolder<Nothing, CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<TCoroutineAwaiter>>>.RentFromCache();
        var coroutineBuilder = new CoroutineAwaiterMethodBuilder<TCoroutineAwaiter>(in coroutineAwaiter, stateMachineHolder);
        var stateMachine = new CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<TCoroutineAwaiter>>(coroutineBuilder) {
            _state = -1
        };
        stateMachineHolder.StateMachine = stateMachine;

        // We cannot assign the context directly to the state machine box,
        // because we do not want to implictly bequest the async iterator
        ref var stateMachineHolderCoroutineContext = ref stateMachineHolder._coroutineContext;
        stateMachineHolderCoroutineContext.InheritContext(in contextToBequest);

        contextToBequest.SetResultStateMachine(stateMachineHolder);
        stateMachineHolder.MoveNext();
        return new Coroutine(new ValueTask(stateMachineHolder, stateMachineHolder.Version));
    }

    internal static Coroutine<TResult> MakeChildCoroutine<TCoroutineAwaiter, TResult>(ref TCoroutineAwaiter coroutineAwaiter, ref CoroutineContext contextToBequest)
        where TCoroutineAwaiter : struct, ICriticalNotifyCompletion, ICoroutineAwaiter<TResult>
    {
        Debug.Assert(contextToBequest.BequesterOrigin == CoroutineContextBequesterOrigin.ChildCoroutine);
        var stateMachineHolder = CoroutineStateMachineHolder<TResult, CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<TCoroutineAwaiter, TResult>, TResult>>.RentFromCache();
        var coroutineBuilder = new CoroutineAwaiterMethodBuilder<TCoroutineAwaiter, TResult>(in coroutineAwaiter, stateMachineHolder);
        var stateMachine = new CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<TCoroutineAwaiter, TResult>, TResult>(coroutineBuilder) {
            _state = -1
        };
        stateMachineHolder.StateMachine = stateMachine;

        // We cannot assign the context directly to the state machine box,
        // because we do not want to implictly bequest the async iterator
        ref var stateMachineHolderCoroutineContext = ref stateMachineHolder._coroutineContext;
        stateMachineHolderCoroutineContext.InheritContext(in contextToBequest);

        contextToBequest.SetResultStateMachine(stateMachineHolder);
        stateMachineHolder.MoveNext();
        return new Coroutine<TResult>(new ValueTask<TResult>(stateMachineHolder, stateMachineHolder.Version));
    }
}
