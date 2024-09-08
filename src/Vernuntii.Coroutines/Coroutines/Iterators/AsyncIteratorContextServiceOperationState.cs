namespace Vernuntii.Coroutines.Iterators;

[Flags]
public enum AsyncIteratorContextServiceOperationState
{
    Uninitialized = 0,
    AwaiterCompletionNotifierRequired = 1,
    ArgumentSupplied = 2,
    AwaiterCompletionNotifierSupplied = 4
}
