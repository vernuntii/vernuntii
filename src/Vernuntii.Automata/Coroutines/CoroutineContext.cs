using System.Diagnostics;

namespace Vernuntii.Coroutines;

internal class CoroutineContext
{
    private int _nodesCount;

    internal int NodesCount => _nodesCount;

    public int AddCoroutineNode()
    {
        return Interlocked.Increment(ref _nodesCount);
    }

    public void RemoveCoroutineNode()
    {
        Interlocked.Decrement(ref _nodesCount);
    }
}
