using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionPersistence;

public class VersionCachePartEqualityComparer : IEqualityComparer<VersionCachePart>
{
    public static readonly VersionCachePartEqualityComparer InvariantCulture = new VersionCachePartEqualityComparer(StringComparison.InvariantCulture);
    public static readonly VersionCachePartEqualityComparer InvariantCultureIgnoreCase = new VersionCachePartEqualityComparer(StringComparison.InvariantCultureIgnoreCase);

    private readonly StringComparer _stringComparer;

    public VersionCachePartEqualityComparer(StringComparison stringComparison) =>
        _stringComparer = StringComparer.FromComparison(stringComparison);

    public bool Equals(VersionCachePart? x, VersionCachePart? y) =>
        ReferenceEquals(x, y)
        || (x is not null && y is not null && _stringComparer.Equals(x.Name, y.Name));

    public int GetHashCode([DisallowNull] VersionCachePart obj) =>
        _stringComparer.GetHashCode(obj.Name);
}
