namespace Vernuntii.Coroutines.Iterators;

[Flags]
public enum AsyncIteratorContextServiceOperationState
{
    AwaiterCompletionNotifierRequired = 1,
    ArgumentSupplied = 2,
    AwaiterCompletionNotifierSupplied = 4
}
