using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer.Parser.Parsers;
using Vernuntii.SemVer.Parser.Slicing;

namespace Vernuntii.SemVer.Parser
{
    /// <summary>
    /// Implements the default parser for a compatible SemVer string.
    /// </summary>
    public record SemanticVersionParser : ISemanticVersionParser
    {
        /// <summary>
        /// A strict version parser of the Semantic Versioning specification.
        /// </summary>
        public readonly static SemanticVersionParser Strict = new SemanticVersionParser();

        /// <summary>
        /// A strict version parser of the Semantic Versioning specification.
        /// If the parser hits invalid leading zeros or invalid alpha-numerics
        /// then they get erased.
        /// </summary>
        public readonly static SemanticVersionParser Erase = new SemanticVersionParser() {
            VersionParser = NumericIdentifierParser.Erase,
            BuildParser = BuildIdentifierParser.Erase,
            PreReleaseParser = PreReleaseIdentifierParser.Erase,
        };

        /// <summary>
        /// Prefix validator. Default is to allowing "v" as prefix.
        /// </summary>
        public IPrefixValidator PrefixValidator {
            get => _prefixValidator;
            init => _prefixValidator = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        public INumericIdentifierParser VersionParser {
            get => _numericIdentifierParser;
            init => _numericIdentifierParser = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        public IDottedIdentifierParser PreReleaseParser {
            get => _preReleaseIdentifierParser;
            init => _preReleaseIdentifierParser = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        public IDottedIdentifierParser BuildParser {
            get => _buildIdentifierParser;
            init => _buildIdentifierParser = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// A semantic version string slicer.
        /// </summary>
        public ISemanticVersionSlicer Slicer {
            get => _slicer;
            init => _slicer = value ?? throw new ArgumentNullException(nameof(value));
        }

        private ISemanticVersionSlicer _slicer = SemanticVersionSlicer.Default;
        private IPrefixValidator _prefixValidator = PrefixAllowlist.Default;
        private INumericIdentifierParser _numericIdentifierParser = NumericIdentifierParser.Strict;
        private IDottedIdentifierParser _preReleaseIdentifierParser = PreReleaseIdentifierParser.Strict;
        private IDottedIdentifierParser _buildIdentifierParser = BuildIdentifierParser.Strict;

        /// <inheritdoc/>
        public bool TryParse(
            string? value,
            out string? prefix,
            [NotNullWhen(true)] out uint? major,
            [NotNullWhen(true)] out uint? minor,
            [NotNullWhen(true)] out uint? patch,
            out IEnumerable<string>? preReleaseIdentifiers,
            out IEnumerable<string>? buildIdentifiers)
        {
            if (value is null) {
                goto prefix;
            }

            if (!Slicer.TrySlice(value, out prefix, out var majorString, out var minorString, out var patchString, out var preReleaseString, out var buildString)) {
                goto major;
            }

            if (!PrefixValidator.ValidatePrefix(prefix)) {
                goto major;
            }

            if (VersionParser.TryParseNumericIdentifier(majorString).DeconstructFailure(out major)) {
                goto major;
            }

            if (VersionParser.TryParseNumericIdentifier(minorString).DeconstructFailure(out minor)) {
                goto minor;
            }

            if (VersionParser.TryParseNumericIdentifier(patchString).DeconstructFailure(out patch)) {
                goto patch;
            }

            switch (PreReleaseParser.TryParseDottedIdentifier(preReleaseString).Deconstruct(out preReleaseIdentifiers)) {
                case IdentifierParseResultState.Null:
                case IdentifierParseResultState.ValidParse:
                    break;
                case IdentifierParseResultState.Empty:
                case IdentifierParseResultState.WhiteSpace:
                    goto preRelease;
                case IdentifierParseResultState.InvalidParse:
                    goto build;
            }

            switch (BuildParser.TryParseDottedIdentifier(buildString).Deconstruct(out buildIdentifiers)) {
                case IdentifierParseResultState.Null:
                case IdentifierParseResultState.ValidParse:
                    break;
                case IdentifierParseResultState.Empty:
                case IdentifierParseResultState.WhiteSpace:
                    goto build;
                default:
                    goto exit;
            }

            return true;

            prefix:
            prefix = null;
            major:
            major = null;
            minor:
            minor = null;
            patch:
            patch = null;
            preRelease:
            preReleaseIdentifiers = null;
            build:
            buildIdentifiers = null;
            exit:
            return false;
        }
    }
}
