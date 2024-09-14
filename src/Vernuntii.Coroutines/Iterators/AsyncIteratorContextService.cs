namespace Vernuntii.Coroutines.Iterators;

internal class AsyncIteratorContextService(AsyncIteratorContextServiceOperation initialOperation)
{
    private AsyncIteratorContextServiceOperation _currentOperation = initialOperation;

    public ref AsyncIteratorContextServiceOperation CurrentOperation => ref _currentOperation;
}
