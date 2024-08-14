namespace Vernuntii.Coroutines;

internal interface ICoroutineHandler
{
    void HandleChildCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutine) where TCoroutineAwaiter : IChildCoroutine;
    void HandleSiblingCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutine) where TCoroutineAwaiter : ISiblingCoroutine;
}
