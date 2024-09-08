namespace Vernuntii.Coroutines;

internal interface IChildCoroutine
{
    void InheritCoroutineContext(in CoroutineContext context);
    void StartCoroutine();
}
