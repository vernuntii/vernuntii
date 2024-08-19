namespace Vernuntii.Coroutines;

internal interface IChildCoroutine
{
    void InheritCoroutineNode(ref CoroutineStackNode coroutineNode);
    void StartCoroutine();
}
