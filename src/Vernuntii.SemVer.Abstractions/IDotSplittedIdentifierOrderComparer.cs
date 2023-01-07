namespace Vernuntii.SemVer;

/// <summary>
/// A comparer for an identifier of a semantic version.
/// </summary>
public interface IDotSplittedIdentifierOrderComparer : IComparer<IEnumerable<string>>, IComparer<string>
{
    /// <inheritdoc cref="IComparer{T}.Compare(T, T)"/>
    int Compare(IEnumerable<string>? dotSplittedIdentifiers, string? otherDottedIdentifier);

    /// <inheritdoc cref="IComparer{T}.Compare(T, T)"/>
    int Compare(string? dottedIdentifier, IEnumerable<string>? otherDotSplittedIdentifiers);
}
