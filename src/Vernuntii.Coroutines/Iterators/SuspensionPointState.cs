namespace Vernuntii.Coroutines.Iterators;

[Flags]
public enum SuspensionPointState
{
    Uninitialized = 0,
    AwaiterCompletionNotifierRequired = 1,
    ArgumentSupplied = 2,
    AwaiterCompletionNotifierSupplied = 4,
    CoroutineAwaiterSupplied = 8
}
