namespace Vernuntii.Coroutines;

internal interface IEntryCoroutine : ICoroutine
{
    void MarkCoroutineAsHandled();
}
