using Vernuntii.SemVer.Parser;

namespace Vernuntii.SemVer;

/// <summary>
/// Provides an instance of <see cref="ISemanticVersionParser"/>.
/// </summary>
public interface ISemanticVersionParserProvider
{
    /// <summary>
    /// Semantic version parser.
    /// </summary>
    ISemanticVersionParser Parser { get; }
}
