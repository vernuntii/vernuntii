using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer.Parser.Normalization;
using static Vernuntii.SemVer.Parser.Parsers.IdentifierParser;

namespace Vernuntii.SemVer.Parser.Parsers
{
    /// <summary>
    /// Build identifier parser.
    /// </summary>
    public sealed class BuildIdentifierParser : IDottedIdentifierParser
    {
        /// <summary>
        /// Build identifier parser with strict presets.
        /// </summary>
        public static readonly BuildIdentifierParser Strict = new(IdentifierParser.Strict);

        /// <summary>
        /// Build identifier parser with erasing aspects: erases invalid leading zeros and invalid alpha-numerics.
        /// </summary>
        public static readonly BuildIdentifierParser Erase = new(IdentifierParser.Erase);

        private readonly IdentifierParser _identifierParser;

        internal BuildIdentifierParser(IdentifierParser identifierParser) => _identifierParser = identifierParser;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="normalizer"></param>
        public BuildIdentifierParser(ISemanticVersionNormalizer normalizer)
            : this(new IdentifierParser(normalizer)) { }

        private bool TryParseNonEmptyBuild(string build, [NotNullWhen(true)] out IEnumerable<string>? result) =>
            _identifierParser.TryParseDottedIdentifier(build, out result);

        /// <inheritdoc/>
        public INullableIdentifierParseResult<IEnumerable<string>> TryParseDottedIdentifier(string? dottedIdentifier) =>
            TryParseIdentifier<IEnumerable<string>>(dottedIdentifier, TryParseNonEmptyBuild, allowNull: true);
    }
}
