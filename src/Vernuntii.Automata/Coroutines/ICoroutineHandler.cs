namespace Vernuntii.Coroutines;

internal interface ICoroutineHandler
{
    void HandleChildCoroutine(ref Coroutine.CoroutineAwaiter coroutineAwaiter);
    void HandleDirectCoroutine([NotNull] CoroutineArgumentReceiverDelegate argumentReceiverDelegate);
}
