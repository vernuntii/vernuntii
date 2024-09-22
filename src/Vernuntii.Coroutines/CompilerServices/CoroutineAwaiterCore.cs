using System.Diagnostics;
using Vernuntii.Coroutines.Iterators;
using Vernuntii.Coroutines.TO_BE_DELETED;

namespace Vernuntii.Coroutines.CompilerServices;

internal delegate ref TAwaiter GetAwaiterDelegate<TCoroutineAwaiter, TAwaiter>(ref TCoroutineAwaiter coroutineAwaiter);

internal class CoroutineAwaiterCore
{
    public static void RenewStateMachineCoroutineAwaiter<TStateMachine, TCoroutineAwaiter, TAwaiter, TValueTaskAccessor, TResult>(
        IAsyncIteratorStateMachineHolder theirStateMachineHolder,
        in SuspensionPoint ourSuspensionPoint,
        ref SuspensionPoint theirSuspensionPoint,
        GetAwaiterDelegate<TCoroutineAwaiter, TAwaiter> getAwaiterDelegate)
        where TStateMachine : IAsyncStateMachine
        where TCoroutineAwaiter : ICriticalNotifyCompletion, IRelativeCoroutine
        where TValueTaskAccessor : IValueTaskAccessor
    {
        var stateMachineHolder = Unsafe.As<CoroutineStateMachineHolder<TResult, TStateMachine>>(theirStateMachineHolder);
        Debug.Assert(stateMachineHolder.StateMachine is not null);
        ref var coroutineAwaiter = ref CoroutineStateMachineCoroutineAwaiterAccessor<TStateMachine, TCoroutineAwaiter>.GetCoroutineAwaiter(ref stateMachineHolder.StateMachine);

        switch (coroutineAwaiter.CoroutineAction) {
            case CoroutineAction.Sibling:
                ref var valueTaskAccessor = ref Unsafe.As<TAwaiter, TValueTaskAccessor>(ref getAwaiterDelegate(ref coroutineAwaiter));
                var completionSource = ManualResetCoroutineCompletionSource<TResult>.RentFromCache();
                valueTaskAccessor._obj = completionSource;
                valueTaskAccessor._token = completionSource.Version;
                Debug.Assert(ourSuspensionPoint._argument is not null);
                theirSuspensionPoint.SupplyArgument(ourSuspensionPoint._argumentKey, ourSuspensionPoint._argument, completionSource);
                coroutineAwaiter.MarkCoroutineAsActedOn();
                theirSuspensionPoint.SupplyAwaiterCriticalCompletionNotifier(ref coroutineAwaiter);
                //new object[]{ ourSuspensionPoint, theirSuspensionPoint}.Dump();
                return;
            default:
                throw new NotSupportedException();
        }
    }
}
