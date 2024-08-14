namespace Vernuntii.Coroutines;

internal interface IChildCoroutine
{
    bool IsChildCoroutine { get; }

    void PropagateCoroutineNode(ref CoroutineStackNode coroutineNode);
    void StartStateMachine();
}
