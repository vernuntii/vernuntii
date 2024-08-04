namespace Vernuntii.Coroutines;

internal class CoroutineContext
{
    private int _nodesCount;

    internal int NodesCount => _nodesCount;

    public void AddCoroutineNode()
    {
        Interlocked.Increment(ref _nodesCount);
    }

    public void RemoveCoroutineNode()
    {
        Interlocked.Decrement(ref _nodesCount);
    }
}
