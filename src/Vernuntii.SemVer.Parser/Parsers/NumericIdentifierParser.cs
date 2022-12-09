using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Vernuntii.SemVer.Parser.Normalization;
using static Vernuntii.SemVer.Parser.Parsers.IdentifierParser;

namespace Vernuntii.SemVer.Parser.Parsers
{
    /// <summary>
    /// Version number parser.
    /// </summary>
    public sealed class NumericIdentifierParser : INumericIdentifierParser
    {
        /// <summary>
        /// A numeric identifier parser with strict presets.
        /// </summary>
        public static readonly NumericIdentifierParser Strict = new(IdentifierParser.Strict);

        /// <summary>
        /// A numeric identifier parser with erasing aspects: erases invalid leading zeros and invalid alpha-numerics.
        /// </summary>
        public static readonly NumericIdentifierParser Erase = new(IdentifierParser.Erase);

        private readonly IdentifierParser _identifierParser;

        internal NumericIdentifierParser(IdentifierParser identifierParser) => _identifierParser = identifierParser;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="normalizer"></param>
        public NumericIdentifierParser(ISemanticVersionNormalizer normalizer)
            : this(new IdentifierParser(normalizer)) { }

        private bool TryParseNonEmptyNumericIdentifier(SemanticVersionPart versionPart, ReadOnlyMemory<char> numericIdentifierSpan, out uint versionNumber)
        {
            var success = _identifierParser.TryResolveFaults(
                versionPart,
                numericIdentifierSpan,
                value => SearchFaults(
                    value.Span,
                    lookupBackslashZero: true,
                    lookupSingleZero: true,
                    lookupNumeric: true),
                out var result);

            if (!success) {
                versionNumber = 0;
                return false;
            }

            return uint.TryParse(result.Span, NumberStyles.None, NumberFormatInfo.InvariantInfo, out versionNumber);
        }

        private bool TryParseNonEmptyNumericIdentifier(SemanticVersionPart versionPart, string? numericIdentifierString, [NotNullWhen(true)] out uint? versionNumber)
        {
            if (TryParseNonEmptyNumericIdentifier(versionPart, numericIdentifierString.AsMemory(), out var result)) {
                versionNumber = result;
                return true;
            }

            versionNumber = null;
            return false;
        }

        /// <inheritdoc/>
        public IRequiredIdentifierParseResult<uint?> TryParseNumericIdentifier(SemanticVersionPart versionPart, string? numericIdentifier) =>
            TryParseIdentifier<uint?>(versionPart, numericIdentifier, TryParseNonEmptyNumericIdentifier, allowNull: false);
    }
}
