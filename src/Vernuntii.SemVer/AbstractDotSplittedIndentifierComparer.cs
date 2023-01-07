using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer.Parser;
using Vernuntii.SemVer.Parser.Parsers;

namespace Vernuntii.SemVer;

internal abstract class AbstractDotSplittedIndentifierComparer : IDotSplittedIdentifierComparer
{
    private static readonly IEnumerable<string> s_emptyIdentifiers = Array.Empty<string>();

    /// <summary>
    /// The default string comparer for the identifier. Is equivalent to <see cref="StringComparer.OrdinalIgnoreCase"/>.
    /// </summary>
    protected StringComparer StringComparer => _stringComparer ??= StringComparer.FromComparison(StringComparisonType);

    /// <summary>
    /// The default string comparison for the identifier. Is equivalent to <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    protected abstract StringComparison StringComparisonType { get; }

    protected abstract SemanticVersionPart ParsingVersionPart { get; }
    protected abstract IDottedIdentifierParser IdentifierParser { get; }

    private StringComparer? _stringComparer;

    /// <summary>
    /// Compares identifiers.
    /// </summary>
    protected abstract int CompareIdentifiers(IEnumerable<string> identifiers, IEnumerable<string> otherIdentifiers);

    /// <inheritdoc/>
    public int Compare(IEnumerable<string>? x, IEnumerable<string>? y)
    {
        x ??= s_emptyIdentifiers;
        y ??= s_emptyIdentifiers;
        return CompareIdentifiers(x, y);
    }

    /// <inheritdoc/>
    public int Compare(string? x, string? y)
    {
        var identifiers = SemanticVersionBuilder.ParseDottedIdentifier(ParsingVersionPart, IdentifierParser, x);
        var otherIdentifiers = SemanticVersionBuilder.ParseDottedIdentifier(ParsingVersionPart, IdentifierParser, y);
        return Compare(identifiers, otherIdentifiers);
    }

    /// <inheritdoc/>
    public bool Equals(IEnumerable<string>? x, IEnumerable<string>? y) =>
        Compare(x, y) == 0;

    /// <inheritdoc/>
    public bool Equals(IEnumerable<string>? dotSplittedIdentifiers, string? otherDottedIdentifier)
    {
        dotSplittedIdentifiers ??= s_emptyIdentifiers;
        var otherDotSplittedIdentifiers = SemanticVersionBuilder.ParseDottedIdentifier(ParsingVersionPart, IdentifierParser, otherDottedIdentifier);
        return Equals(dotSplittedIdentifiers, otherDotSplittedIdentifiers);
    }

    /// <inheritdoc/>
    public bool Equals(string? dottedIdentifier, IEnumerable<string>? otherDotSplittedIdentifiers)
    {
        var dotSplittedIdentifiers = SemanticVersionBuilder.ParseDottedIdentifier(ParsingVersionPart, IdentifierParser, dottedIdentifier);
        otherDotSplittedIdentifiers ??= s_emptyIdentifiers;
        return Equals(dotSplittedIdentifiers, otherDotSplittedIdentifiers);
    }

    /// <inheritdoc/>
    public bool Equals(string? x, string? y)
    {
        var identifiers = SemanticVersionBuilder.ParseDottedIdentifier(ParsingVersionPart, IdentifierParser, x);
        var otherIdentifiers = SemanticVersionBuilder.ParseDottedIdentifier(ParsingVersionPart, IdentifierParser, y);
        return Equals(identifiers, otherIdentifiers);
    }

    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] IEnumerable<string> dotSplittedIdentifiers) =>
        SemanticVersion.CombineDotSplitted(dotSplittedIdentifiers).GetHashCode(StringComparisonType);

    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] string dottedIdentifier) => 
        dottedIdentifier.GetHashCode(StringComparisonType);
}
