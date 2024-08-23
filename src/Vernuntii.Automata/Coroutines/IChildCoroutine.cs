namespace Vernuntii.Coroutines;

internal interface IChildCoroutine
{
    void InheritCoroutineContext(ref CoroutineContext coroutineContext);
    void StartCoroutine();
}
