namespace Vernuntii.Coroutines;

internal interface ICoroutineHandler
{
    void HandleChildCoroutine(ref Coroutine.CoroutineAwaiter coroutineAwaiter);
    //void HandleChildCoroutine<TAwaiter>(ref TAwaiter awaiter);

    void HandleDirectCoroutine([NotNull] CoroutineArgumentReceiverDelegate argumentReceiverDelegate);
}
