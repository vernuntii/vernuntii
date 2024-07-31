using System.Runtime.CompilerServices;
using Collections.Pooled;

namespace Vernuntii.Coroutines;

internal class CoroutineContext
{
    //private const int _defaultNodesCapacity = 4;

    //private PooledList<CoroutineContextStackNode> _nodes = new(_defaultNodesCapacity);
    //private int _nodesCapacity;
    private int _nodesCount;

    internal int NodesCount => _nodesCount;

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private void EnsureNodesCapacity()
    //{
    //    if (_nodesCount == _nodesCapacity) {
    //        _nodes.Capacity = _nodesCapacity *= 2;
    //    }
    //}

    public void AddNode(in CoroutineStackNode node)
    {
        //EnsureNodesCapacity();
        _nodesCount++;
    }

    //public void Test()
    //{
    //    CollectionsMarshal.AsSpan(_nodes);
    //}

    //public struct CoroutineContextStackNode();
}
