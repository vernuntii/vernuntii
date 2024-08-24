namespace Vernuntii.Coroutines;

internal static class CoroutineContextExtensions
{
    internal static void UpdateBequeathBehaviour(this ref CoroutineContext coroutineContext, CoroutineContextBequeathBehaviour originatingFrom)
    {
        //ref var chainStates = ref coroutineContext._chainStates;

        //if (originatingFrom == CoroutineContextBequeathBehaviour.OwnedBySibling) {
        //    chainStates = chainStates & ~CoroutineContextBequeathBehaviour.OwnedByChild | originatingFrom;
        //} else if (originatingFrom == CoroutineContextBequeathBehaviour.OwnedByChild) {
        //    chainStates = chainStates & ~CoroutineContextBequeathBehaviour.OwnedBySibling | originatingFrom | CoroutineContextBequeathBehaviour.OnceOwnedByChild;
        //}
    }
}
