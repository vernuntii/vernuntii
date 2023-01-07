namespace Vernuntii.SemVer;

/// <summary>
/// Comparer that has <see cref="IDotSplittedIdentifierOrderComparer"/>
/// and <see cref="IDotSplittedIdentifierEqualityComparer"/> capabilities.
/// </summary>
public interface IDotSplittedIdentifierComparer : IDotSplittedIdentifierOrderComparer, IDotSplittedIdentifierEqualityComparer
{
}
