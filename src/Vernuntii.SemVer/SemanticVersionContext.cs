using Vernuntii.SemVer.Parser;
using Vernuntii.SemVer.Parser.Parsers;

namespace Vernuntii.SemVer;

public record SemanticVersionContext : ISemanticVersionContext
{
    public static readonly ISemanticVersionContext Strict = new SemanticVersionContext() {
        Parser = SemanticVersionParser.Strict,
        PrefixValidator = SemanticVersionParser.Strict.PrefixValidator,
        VersionParser = SemanticVersionParser.Strict.VersionParser,
        PreReleaseParser = SemanticVersionParser.Strict.PreReleaseParser,
        BuildParser = SemanticVersionParser.Strict.BuildParser
    };

    public static readonly ISemanticVersionContext Erase = new SemanticVersionContext() {
        Parser = SemanticVersionParser.Erase,
        PrefixValidator = SemanticVersionParser.Erase.PrefixValidator,
        VersionParser = SemanticVersionParser.Erase.VersionParser,
        PreReleaseParser = SemanticVersionParser.Erase.PreReleaseParser,
        BuildParser = SemanticVersionParser.Erase.BuildParser
    };

    public required ISemanticVersionParser Parser { get; init; }
    public required IPrefixValidator PrefixValidator { get; init; }
    public required INumericIdentifierParser VersionParser { get; init; }
    public required IDottedIdentifierParser PreReleaseParser { get; init; }
    public required IDottedIdentifierParser BuildParser { get; init; }
}
