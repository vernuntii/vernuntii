using System.Globalization;
using Vernuntii.SemVer.Parser;
using Vernuntii.SemVer.Parser.Parsers;

namespace Vernuntii.SemVer
{
    /// <summary>
    /// Builds an instance of type <see cref="SemanticVersion"/>.
    /// </summary>
    public sealed class SemanticVersionBuilder
    {
        private static string? CombineDotSplitted(IReadOnlyCollection<string>? values) =>
            values is null
            ? null
            : (values.Count == 0
                ? null
                : string.Join('.', values));

        private static string StringifyVersionNumber(uint versionNumber) =>
            versionNumber.ToString(CultureInfo.InvariantCulture);

        internal static void ValidatePrefix(IPrefixValidator validator, string? prefix)
        {
            if (!validator.ValidatePrefix(prefix)) {
                throw new SemanticVersionBuilderException("Prefix validation failed");
            }
        }

        internal static uint ParseVersionNumber(IVersionNumberParser parser, uint versionNumber)
        {
            if (parser.TryParseVersionNumber(StringifyVersionNumber(versionNumber)).DeconstructFailure(out var wrappedVersionNumber)) {
                throw new SemanticVersionBuilderException("Version number is not valid");
            }

            return wrappedVersionNumber.Value;
        }

        internal static IReadOnlyList<string> ParseDottedIdentifier(
            IDottedIdentifierParser dottedIdentifierParser,
            string? newDottedIdentifier)
        {
            if (dottedIdentifierParser.TryParseDottedIdentifier(newDottedIdentifier).DeconstructFailure(out var identifierEnumerable, SemanticVersion.EmptyIdentifiers)) {
                throw new SemanticVersionBuilderException("Dotted identifier is not valid");
            }

            return identifierEnumerable is string[] array ? array : identifierEnumerable.ToArray();
        }

        internal static IReadOnlyList<string> ParseDotSplittedIdentifier(
            IDottedIdentifierParser dottedIdentifierParser,
            IReadOnlyCollection<string>? newdotSplittedIdentifiers) =>
            ParseDottedIdentifier(dottedIdentifierParser, CombineDotSplitted(newdotSplittedIdentifiers));

        internal SemanticVersion BaseVersion { get; }

        uint? _major, _minor, _patch;
        IReadOnlyCollection<string>? _preReleaseIdentifiers, _buildIdentifiers;
        string? _prefix, _preRelease, _build;
        bool _withPrefix, _withPreRelease, _withBuild;

        /// <summary>
        /// Creates an instance of <see cref="SemanticVersionBuilder"/>.
        /// </summary>
        /// <param name="baseVersion"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SemanticVersionBuilder(SemanticVersion baseVersion) =>
            BaseVersion = baseVersion ?? throw new ArgumentNullException(nameof(baseVersion));

        /// <summary>
        /// Creates an instance of <see cref="SemanticVersionBuilder"/>.
        /// </summary>
        public SemanticVersionBuilder() =>
            BaseVersion = SemanticVersion.Zero;

        /// <inheritdoc/>
        public SemanticVersionBuilder Prefix(string? prefix)
        {
            _prefix = prefix;
            _withPrefix = true;
            return this;
        }

        /// <inheritdoc/>
        public SemanticVersionBuilder Major(uint major)
        {
            _major = major;
            return this;
        }

        /// <inheritdoc/>
        public SemanticVersionBuilder Minor(uint minor)
        {
            _minor = minor;
            return this;
        }

        /// <inheritdoc/>
        public SemanticVersionBuilder Patch(uint patch)
        {
            _patch = patch;
            return this;
        }

        /// <inheritdoc/>
        public SemanticVersionBuilder PreRelease(string? preRelease)
        {
            _preRelease = preRelease == string.Empty ? null : preRelease;
            _preReleaseIdentifiers = null;
            _withPreRelease = true;
            return this;
        }

        /// <inheritdoc/>
        public SemanticVersionBuilder PreRelease(IReadOnlyCollection<string>? preReleaseIdentifiers)
        {
            _preRelease = null;
            _preReleaseIdentifiers = preReleaseIdentifiers;
            _withPreRelease = true;
            return this;
        }

        /// <inheritdoc/>
        public SemanticVersionBuilder Build(string? build)
        {
            _build = build == string.Empty ? null : build;
            _buildIdentifiers = null;
            _withBuild = true;
            return this;
        }

        /// <inheritdoc/>
        public SemanticVersionBuilder Build(IReadOnlyCollection<string>? buildIdentifiers)
        {
            _build = null;
            _buildIdentifiers = buildIdentifiers;
            _withBuild = true;
            return this;
        }

        private SemanticVersion BuildByParsing(ISemanticVersionParser parser)
        {
            string? prefix;

            if (_withPrefix) {
                ValidatePrefix(parser.PrefixValidator, _prefix);
                prefix = _prefix;
            } else {
                prefix = BaseVersion.Prefix;
            }

            return new SemanticVersion(
                parser,
                prefix,
                SelectVersionNumber(_major, BaseVersion.Major),
                SelectVersionNumber(_minor, BaseVersion.Minor),
                SelectVersionNumber(_patch, BaseVersion.Patch),
                SelectDottedIdentifiers(parser.PreReleaseParser, _withPreRelease, BaseVersion.PreReleaseIdentifiers, _preRelease, _preReleaseIdentifiers),
                SelectDottedIdentifiers(parser.BuildParser, _withBuild, BaseVersion.BuildIdentifiers, _build, _buildIdentifiers));

            uint SelectVersionNumber(uint? newVersionNumber, uint baseVersionNumber)
            {
                if (!newVersionNumber.HasValue) {
                    return baseVersionNumber;
                }

                return ParseVersionNumber(parser.VersionNumberParser, newVersionNumber.Value);
            }

            IReadOnlyList<string> SelectDottedIdentifiers(
                IDottedIdentifierParser dottedIdentifierParser,
                bool useNewDottedIdentifier,
                IReadOnlyList<string> baseDottedIdentifiers,
                string? newDottedIdentifier,
                IReadOnlyCollection<string>? newDotSplittedIdentifiers)
            {
                if (!useNewDottedIdentifier) {
                    return baseDottedIdentifiers;
                }

                if (newDottedIdentifier != null) {
                    return ParseDottedIdentifier(dottedIdentifierParser, newDottedIdentifier);
                }

                return ParseDotSplittedIdentifier(dottedIdentifierParser, newDotSplittedIdentifiers);
            }
        }

        /// <summary>
        /// Builds the version.
        /// </summary>
        /// <param name="parser"></param>
        public SemanticVersion BuildVersion(ISemanticVersionParser parser) =>
            BuildByParsing(parser);

        /// <summary>
        /// Builds the version.
        /// </summary>
        public SemanticVersion BuildVersion() =>
            BuildVersion(BaseVersion.Parser);

        /// <inheritdoc/>
        public static implicit operator SemanticVersion(SemanticVersionBuilder builder) =>
            builder.BuildVersion();
    }
}
