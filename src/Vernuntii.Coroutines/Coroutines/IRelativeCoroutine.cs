namespace Vernuntii.Coroutines;

internal interface IRelativeCoroutine : IChildCoroutine, ISiblingCoroutine
{
    bool IsChildCoroutine { get; }
    bool IsSiblingCoroutine { get; }
}
