namespace Vernuntii.Coroutines;

internal interface ICoroutineActor
{
    void ActOnChildCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutine) where TCoroutineAwaiter : IChildCoroutine;
    void ActOnSiblingCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutine) where TCoroutineAwaiter : ISiblingCoroutine;
}
