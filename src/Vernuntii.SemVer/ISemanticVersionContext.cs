using Vernuntii.SemVer.Parser;
using Vernuntii.SemVer.Parser.Parsers;

namespace Vernuntii.SemVer;

/// <summary>
/// Represents a persistent semantic-version context. It purpose is to keep the same parser informations intact between new semantic version instances.
/// </summary>
public interface ISemanticVersionContext
{
    /// <inheritdoc cref="SemanticVersionParser"/>.
    ISemanticVersionParser Parser { get; }

    /// <inheritdoc cref="SemanticVersionParser.PrefixValidator"/>.
    IPrefixValidator PrefixValidator { get; }

    /// <inheritdoc cref="SemanticVersionParser.VersionParser"/>.
    INumericIdentifierParser VersionParser { get; }

    /// <inheritdoc cref="SemanticVersionParser.PreReleaseParser"/>.
    IDottedIdentifierParser PreReleaseParser { get; }

    /// <inheritdoc cref="SemanticVersionParser.BuildParser"/>.
    IDottedIdentifierParser BuildParser { get; }
}
