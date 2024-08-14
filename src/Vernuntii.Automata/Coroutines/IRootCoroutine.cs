namespace Vernuntii.Coroutines;

internal interface IRootCoroutine : IChildCoroutine, ISiblingCoroutine
{
    void MarkCoroutineAsHandled();
}
