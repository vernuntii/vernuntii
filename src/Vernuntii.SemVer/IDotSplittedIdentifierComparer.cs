namespace Vernuntii.SemVer;

/// <summary>
/// A comparer for an identifier of a semantic version.
/// </summary>
public interface IDotSplittedIdentifierComparer : IComparer<IEnumerable<string>>, IEqualityComparer<IEnumerable<string>>, IComparer<string>, IEqualityComparer<string>
{
    bool Equals(IEnumerable<string>? dotSplittedIdentifiers, string? otherDottedIdentifier);
    bool Equals(string? dottedIdentifier, IEnumerable<string>? otherDotSplittedIdentifiers);
}
