namespace Vernuntii.Coroutines;

internal interface ICoroutineMethodBuilderAwareCoroutine
{
    void PropagateCoroutineNode(ref CoroutineStackNode coroutineNode);

    void StartStateMachine();

    void MarkCoroutineAsHandled();
}
