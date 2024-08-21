namespace Vernuntii.Coroutines;

internal interface ICoroutineMethodBuilderBox
{
    void InheritCoroutineNode(ref CoroutineStackNode coroutineNode);
    void StartCoroutine();
}
