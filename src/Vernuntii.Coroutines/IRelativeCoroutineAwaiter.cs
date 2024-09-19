using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

internal interface IRelativeCoroutineAwaiter : IRelativeCoroutine
{
    /// <summary>
    /// Replaces the coroutine awaiter of the state machine.
    /// </summary>
    /// <typeparam name="TStateMachine"></typeparam>
    /// <param name="stateMachine"></param>
    /// <param name="suspensionPoint">
    /// The suspension point at which the user decided to clone the async iterator. When replacing the coroutine awaiter of the state machine,
    /// we also must adapt the suspension point to reflect the changes we made against the state machine.
    /// </param>
    void ReplaceStateMachineCoroutineAwaiter<TStateMachine>(ref TStateMachine stateMachine, ref SuspensionPoint suspensionPoint) where TStateMachine : IAsyncStateMachine;
}
