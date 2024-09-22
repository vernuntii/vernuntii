namespace Vernuntii.Coroutines.Iterators;

internal class AsyncIteratorContextService(in SuspensionPoint initialSuspensionPoint, bool isAsyncIteratorCloneable)
{
    private readonly bool _isAsyncIteratorCloneable = isAsyncIteratorCloneable;
    internal SuspensionPoint _currentSuspensionPoint = initialSuspensionPoint;

    public bool IsAsyncIteratorCloneable {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isAsyncIteratorCloneable;
    }
}
