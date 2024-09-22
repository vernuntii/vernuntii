using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

internal interface IRelativeCoroutineAwaiter : IRelativeCoroutine
{
    /// <summary>
    /// Replaces the coroutine awaiter of the state machine.
    /// By having any coroutine awaiter implementing <see cref="ReplaceStateMachineCoroutineAwaiter{TStateMachine}(ICoroutineStateMachineHolder, ref SuspensionPoint)"/>
    /// the implementer, which is any of our coroutine awaiter, is capable to identify the position of its type within the instance of <paramref name="theirStateMachineHolder"/>.
    /// Do not assume the called coroutine awaiter (this) being associated with <paramref name="theirStateMachineHolder"/> or <paramref name="theirSuspensionPoint"/> in any kind,
    /// therefore you should NOT call or manipulate any members of the called coroutine awaiter (this).
    /// Do not assume <paramref name="theirSuspensionPoint"/> being a copy of the original async iterator.
    /// </summary>
    /// <typeparam name="TStateMachine"></typeparam>
    /// <param name="stateMachine"></param>
    /// <param name="theirSuspensionPoint">
    /// The suspension point at which the user decided to clone the async iterator. When replacing the coroutine awaiter of the state machine,
    /// we also must adjust the suspension point to reflect the changes we make against the state machine.
    /// </param>
    void RenewStateMachineCoroutineAwaiter<TStateMachine>(
        IAsyncIteratorStateMachineHolder theirStateMachineHolder,
        in SuspensionPoint ourSuspensionPoint,
        ref SuspensionPoint theirSuspensionPoint) where TStateMachine : IAsyncStateMachine;
}
