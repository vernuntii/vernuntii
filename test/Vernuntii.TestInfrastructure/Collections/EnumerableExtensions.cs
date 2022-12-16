namespace Vernuntii.Collections;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Order<T>(this IEnumerable<T> enumerable, IComparer<T> comparer) =>
        enumerable.OrderBy(static item => item, comparer);
}
