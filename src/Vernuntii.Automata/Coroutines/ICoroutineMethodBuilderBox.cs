namespace Vernuntii.Coroutines;

internal interface ICoroutineMethodBuilderBox
{
    void InheritCoroutineContext(ref CoroutineContext coroutineContext);
    void StartCoroutine();
}
