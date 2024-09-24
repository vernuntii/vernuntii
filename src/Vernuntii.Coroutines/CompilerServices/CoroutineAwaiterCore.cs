using System.Diagnostics;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines.CompilerServices;

internal delegate ref TAwaiter GetAwaiterDelegate<TCoroutineAwaiter, TAwaiter>(ref TCoroutineAwaiter coroutineAwaiter);

internal class CoroutineAwaiterCore
{
    public static void RenewStateMachineCoroutineAwaiterCore<TCoroutineAwaiter, TAwaiter, TValueTaskAccessor>(
        in SuspensionPoint ourSuspensionPoint,
        ref SuspensionPoint theirSuspensionPoint,
        ref TCoroutineAwaiter coroutineAwaiter,
        GetAwaiterDelegate<TCoroutineAwaiter, TAwaiter> getAwaiterDelegate)
        where TCoroutineAwaiter : struct, ICriticalNotifyCompletion, IRelativeCoroutine
        where TValueTaskAccessor : IValueTaskAccessor
    {
        switch (coroutineAwaiter.CoroutineAction) {
            case CoroutineAction.Sibling:
                ref var valueTaskAccessor = ref Unsafe.As<TAwaiter, TValueTaskAccessor>(ref getAwaiterDelegate(ref coroutineAwaiter));
                Debug.Assert(ourSuspensionPoint._argumentCompletionSource is not null);
                Debug.Assert(ourSuspensionPoint._argument is not null);
                var completionSource = ourSuspensionPoint._argumentCompletionSource.CreateNew(out var token);
                valueTaskAccessor._obj = completionSource;
                valueTaskAccessor._token = token;
                theirSuspensionPoint.SupplyArgument(ourSuspensionPoint._argumentKey, ourSuspensionPoint._argument, completionSource);
                coroutineAwaiter.MarkCoroutineAsActedOn();
                theirSuspensionPoint.SupplyAwaiterCriticalCompletionNotifier(ref coroutineAwaiter);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public static void RenewStateMachineCoroutineAwaiter<[DAM(StateMachineMemberTypes)] TStateMachine, TCoroutineAwaiter, TAwaiter, TValueTaskAccessor, TResult>(
        IAsyncIteratorStateMachineHolder theirStateMachineHolder,
        in SuspensionPoint ourSuspensionPoint,
        ref SuspensionPoint theirSuspensionPoint,
        GetAwaiterDelegate<TCoroutineAwaiter, TAwaiter> getAwaiterDelegate)
        where TStateMachine : IAsyncStateMachine
        where TCoroutineAwaiter : struct, ICriticalNotifyCompletion, IRelativeCoroutine
        where TValueTaskAccessor : IValueTaskAccessor
    {
        var stateMachineHolder = Unsafe.As<CoroutineStateMachineHolder<TResult, TStateMachine>>(theirStateMachineHolder);
        Debug.Assert(stateMachineHolder.StateMachine is not null);

        if (GlobalRuntimeFeature.IsDynamicCodeSupported) {
            ref var coroutineAwaiter = ref CoroutineStateMachineCoroutineAwaiterAccessor<TStateMachine, TCoroutineAwaiter>.CoroutineAwaiterAccessor.GetValueReference(ref stateMachineHolder.StateMachine);
            RenewStateMachineCoroutineAwaiterCore<TCoroutineAwaiter, TAwaiter, TValueTaskAccessor>(in ourSuspensionPoint, ref theirSuspensionPoint, ref coroutineAwaiter, getAwaiterDelegate);
        } else {
            var stateMachineRef = __makeref(stateMachineHolder.StateMachine);
            var coroutineAwaiterBox = CoroutineStateMachineCoroutineAwaiterAccessor<TStateMachine, TCoroutineAwaiter>.CoroutineAwaiterAccessor.GetValue(stateMachineRef);
            ref var coroutineAwaiter = ref Unsafe.Unbox<TCoroutineAwaiter>(coroutineAwaiterBox);
            RenewStateMachineCoroutineAwaiterCore<TCoroutineAwaiter, TAwaiter, TValueTaskAccessor>(in ourSuspensionPoint, ref theirSuspensionPoint, ref coroutineAwaiter, getAwaiterDelegate);
            CoroutineStateMachineCoroutineAwaiterAccessor<TStateMachine, TCoroutineAwaiter>.CoroutineAwaiterAccessor.SetValue(stateMachineRef, coroutineAwaiterBox);
        }
    }
}
