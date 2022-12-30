namespace Vernuntii.Extensions
{
    internal static class HashCodeExtensions
    {
        public static void AddEnumerable<T>(
            this ref HashCode hashCode,
            IEnumerable<T> enumerable,
            IEqualityComparer<T>? equalityComparer = null) =>
            hashCode.Add(enumerable.GetEnumeratedHashCode(equalityComparer));
    }
}
