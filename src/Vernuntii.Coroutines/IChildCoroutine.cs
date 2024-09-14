namespace Vernuntii.Coroutines;

internal interface IChildCoroutine
{
    void StartCoroutine(in CoroutineContext context);
}
