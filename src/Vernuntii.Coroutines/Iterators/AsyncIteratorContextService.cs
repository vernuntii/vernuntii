namespace Vernuntii.Coroutines.Iterators;

internal class AsyncIteratorContextService(SuspensionPoint initialOperation, bool isAsyncIteratorCloneable)
{
    private readonly bool _isAsyncIteratorCloneable = isAsyncIteratorCloneable;
    private SuspensionPoint _currentOperation = initialOperation;

    public ref SuspensionPoint CurrentOperation => ref _currentOperation;

    public bool IsAsyncIteratorCloneable {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isAsyncIteratorCloneable;
    }
}
