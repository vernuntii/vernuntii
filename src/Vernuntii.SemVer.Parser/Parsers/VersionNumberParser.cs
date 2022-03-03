using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Vernuntii.SemVer.Parser.Normalization;
using static Vernuntii.SemVer.Parser.Parsers.IdentifierParser;

namespace Vernuntii.SemVer.Parser.Parsers
{
    /// <summary>
    /// version number parser.
    /// </summary>
    public sealed class VersionNumberParser : IVersionNumberParser
    {
        /// <summary>
        /// A numeric identifier parser with strict presets.
        /// </summary>
        public readonly static VersionNumberParser Strict = new VersionNumberParser(IdentifierParser.Strict);

        private readonly IdentifierParser _identifierParser;

        internal VersionNumberParser(IdentifierParser identifierParser) => _identifierParser = identifierParser;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="normalizer"></param>
        public VersionNumberParser(ISemanticVersionNormalizer normalizer)
            : this(new IdentifierParser(normalizer)) { }

        private bool TryParseNonEmptyVersionNumber(ReadOnlySpan<char> versionNumberSpan, out uint versionNumber)
        {
            var success = _identifierParser.TryResolveFaults(
                versionNumberSpan,
                value => SearchFaults(
                    value,
                    lookupSingleZero: true,
                    lookupNumeric: true),
                out var result);

            if (!success) {
                versionNumber = 0;
                return false;
            }

            return uint.TryParse(result, NumberStyles.None, NumberFormatInfo.InvariantInfo, out versionNumber);
        }

        private bool TryParseNonEmptyVersionNumber(string? versionNumberString, [NotNullWhen(true)] out uint? versionNumber)
        {
            if (TryParseNonEmptyVersionNumber(versionNumberString.AsSpan(), out var result)) {
                versionNumber = result;
                return true;
            }

            versionNumber = null;
            return false;
        }

        /// <inheritdoc/>
        public IdentifierParseResult<uint?> TryParseVersionNumber(string? versionNumber) =>
            TryParseIdentifier<uint?>(versionNumber, TryParseNonEmptyVersionNumber);
    }
}
