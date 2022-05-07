using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer.Parser.Normalization;
using static Vernuntii.SemVer.Parser.Parsers.IdentifierParser;

namespace Vernuntii.SemVer.Parser.Parsers
{
    /// <summary>
    /// Pre-release identifier parser.
    /// </summary>
    public sealed class PreReleaseIdentifierParser : IDottedIdentifierParser
    {
        /// <summary>
        /// Pre-release identifier parser with strict presets.
        /// </summary>
        public readonly static PreReleaseIdentifierParser Strict = new PreReleaseIdentifierParser(IdentifierParser.Strict);

        /// <summary>
        /// Pre-release identifier parser with erasing aspects: erases invalid leading zeros and invalid alpha-numerics.
        /// </summary>
        public readonly static PreReleaseIdentifierParser Erase = new PreReleaseIdentifierParser(IdentifierParser.Erase);

        private readonly IdentifierParser _identifierParser;

        internal PreReleaseIdentifierParser(IdentifierParser identifierParser) => _identifierParser = identifierParser;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="normalizer"></param>
        public PreReleaseIdentifierParser(ISemanticVersionNormalizer normalizer)
            : this(new IdentifierParser(normalizer)) { }

        private bool TryParseNonEmptyPreRelease(string dottedIdentifier, [NotNullWhen(true)] out IEnumerable<string>? result) =>
            _identifierParser.TryParseDottedIdentifier(dottedIdentifier, out result, lookupSingleZero: true);

        /// <inheritdoc/>
        public INullableIdentifierParseResult<IEnumerable<string>> TryParseDottedIdentifier(string? dottedIdentifier) =>
            TryParseIdentifier<IEnumerable<string>>(dottedIdentifier, TryParseNonEmptyPreRelease, allowNull: true);
    }
}
