namespace Vernuntii.Coroutines;

internal interface ICoroutine
{
    void PropagateCoroutineNode(ref CoroutineStackNode coroutineNode);

    void StartStateMachine();
}
