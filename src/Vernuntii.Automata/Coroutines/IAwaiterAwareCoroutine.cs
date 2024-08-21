namespace Vernuntii.Coroutines;

internal interface IAwaiterAwareCoroutine : IRelativeCoroutine
{
    void MarkCoroutineAsHandled();
}
