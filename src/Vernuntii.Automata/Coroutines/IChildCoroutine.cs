namespace Vernuntii.Coroutines;

internal interface IChildCoroutine
{
    void InheritCoroutineContext(ref CoroutineContext context);
    void StartCoroutine();
}
