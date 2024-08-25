namespace Vernuntii;

[Flags]
public enum CoroutineContextBequesterOrigin
{
    RelativeCoroutine = 0,
    ChildCoroutine = 1,
    SiblingCoroutine = 2,
    ContextBequester = 4
}
