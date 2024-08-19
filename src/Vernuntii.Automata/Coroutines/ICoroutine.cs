namespace Vernuntii.Coroutines;

internal interface ICoroutine : IChildCoroutine, ISiblingCoroutine
{
    bool IsChildCoroutine { get; }
    bool IsSiblingCoroutine { get; }
}
