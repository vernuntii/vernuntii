namespace Vernuntii.Coroutines;

internal interface IAwaitableCoroutine : IRelativeCoroutine
{
    void MarkCoroutineAsHandled();
}
