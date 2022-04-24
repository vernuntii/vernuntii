namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IList{T}"/>.
    /// </summary>
    internal static class ListExtensions
    {
        /// <summary>
        /// Binary search <paramref name="item"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="item"></param>
        /// <param name="comparer"></param>
        /// <returns>-1 if not found</returns>
        public static int BinarySearch<T>(this IReadOnlyList<T> items, T item, IComparer<T> comparer)
        {
            int minIndex = 0;
            int maxIndex = items.Count - 1;

            while (minIndex <= maxIndex) {
                int mid = (minIndex + maxIndex) / 2;
                var compare = comparer.Compare(item, items[mid]);

                if (compare == 0) {
                    return ++mid;
                } else if (compare < 0) {
                    maxIndex = mid - 1;
                } else {
                    minIndex = mid + 1;
                }
            }

            return -1;
        }
    }
}
