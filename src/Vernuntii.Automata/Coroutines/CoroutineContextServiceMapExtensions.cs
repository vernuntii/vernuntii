using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineContextServiceMapExtensions
{
    /// <summary>
    /// Merges the right dictionary into the left dictionary.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static CoroutineContextServiceMap Merge(this CoroutineContextServiceMap? left, CoroutineContextServiceMap right, bool forceNewInstance = false)
    {
        CoroutineContextServiceMap merge;

        if (!forceNewInstance) {
            if (left is null || left.Count == 0) {
                return right;
            }

            if (right.Count == 0) {
                return left;
            }

            merge = new CoroutineContextServiceMap();
        } else {
            merge = new CoroutineContextServiceMap();

            if (left is null || left.Count == 0) {
                merge.Copy(right);
                return merge;
            }

            if (right.Count == 0) {
                return merge;
            }
        }

        var (smaller, greater) = SortByCount(left, right);
        merge.Copy(greater);
        CopyTo(smaller, merge);
        return merge;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        static void CopyTo(CoroutineContextServiceMap from, CoroutineContextServiceMap to)
        {
            for (int i = 0, l = from.Count; i < l; ++i) {
                var meta = from._meta[i];
                if (meta is 0) {
                    continue;
                }

                if (!to.Emplace(from._entries[i].Key, from._entries[i].Value)) {
                    var toEntry = RobinhoodMap<Key, object>.Find(to._entries, to.Hash(from._entries[i].Key));
                    var fromEntry = RobinhoodMap<Key, object>.Find(from._entries, from.Hash(from._entries[i].Key));
                    throw new InvalidOperationException($"""
When attempting to merge a key-value pair into the target dictionary, both key hashes matched but did not pass the equality check due to hash collision or faulty equality check:
To Be Replaced = {toEntry}
To Be Replaced By = {fromEntry}
""");
                }
            }
        }

        static (CoroutineContextServiceMap Smaller, CoroutineContextServiceMap Greater) SortByCount(CoroutineContextServiceMap left, CoroutineContextServiceMap right)
        {
            if (left.Count < right.Count) {
                return (left, right);
            }

            return (right, left);
        }
    }
}
