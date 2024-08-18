namespace Vernuntii.Coroutines;

internal interface IEntryCoroutine : IChildCoroutine, ISiblingCoroutine
{
    void MarkCoroutineAsHandled();
}
