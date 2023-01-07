namespace Vernuntii.SemVer;

/// <summary>
/// An equality comparer for an identifier of a semantic version.
/// </summary>
public interface IDotSplittedIdentifierEqualityComparer : IEqualityComparer<IEnumerable<string>>, IEqualityComparer<string>
{
    /// <inheritdoc cref="IEqualityComparer{T}.Equals(T, T)"/>
    bool Equals(IEnumerable<string>? dotSplittedIdentifiers, string? otherDottedIdentifier);

    /// <inheritdoc cref="IEqualityComparer{T}.Equals(T, T)"/>
    bool Equals(string? dottedIdentifier, IEnumerable<string>? otherDotSplittedIdentifiers);
}
