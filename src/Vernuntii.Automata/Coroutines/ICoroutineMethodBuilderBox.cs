namespace Vernuntii.Coroutines;

internal interface ICoroutineMethodBuilderBox
{
    void InheritCoroutineNode(ref CoroutineStackNode parentNode);
    void StartCoroutine();
}
