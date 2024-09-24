using System.Diagnostics;
using Vernuntii.Coroutines.Iterators;

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
        ref var coroutineAwaiter = ref CoroutineStateMachineCoroutineAwaiterAccessor<TStateMachine, TCoroutineAwaiter>.CoroutineAwaiterAccessor.GetValueReference(ref stateMachineHolder.StateMachine);

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
                return;
            default:
                throw new NotSupportedException();
        }
    }
}
