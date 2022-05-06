namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IEnumerable{T}"/>.
    /// </summary>
    internal static class EnumerableExtensions
    {
        public static int GetEnumeratedHashCode<T>(this IEnumerable<T>? enumerable, IEqualityComparer<T>? equalityComparer = null)
        {
            if (enumerable is null) {
                return 0;
            }

            equalityComparer ??= EqualityComparer<T>.Default;
            var hashCode = new HashCode();

            foreach (var item in enumerable) {
                hashCode.Add(item, equalityComparer);
            }

            return hashCode.ToHashCode();
        }
    }
}
