using System.Text;

namespace Vernuntii.Coroutines.Iterators;

internal static class AsyncIterator
{
    internal static readonly Key s_asyncIteratorKey = new Key(Encoding.ASCII.GetBytes(nameof(AsyncIterator)));
}
