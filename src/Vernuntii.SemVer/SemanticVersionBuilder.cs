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

        internal static uint ParseVersionNumber(INumericIdentifierParser parser, SemanticVersionPart versionPart, uint versionNumber)
        {
            if (parser.TryParseNumericIdentifier(versionPart, StringifyVersionNumber(versionNumber)).DeconstructFailure(out var wrappedVersionNumber)) {
                throw new SemanticVersionBuilderException("Version number is not valid");
            }

            return wrappedVersionNumber.Value;
        }

        internal static IReadOnlyList<string> ParseDottedIdentifier(
            SemanticVersionPart versionPart,
            IDottedIdentifierParser dottedIdentifierParser,
            string? newDottedIdentifier)
        {
            if (dottedIdentifierParser.TryParseDottedIdentifier(versionPart, newDottedIdentifier).DeconstructFailure(out var identifierEnumerable, SemanticVersion.s_emptyIdentifiers)) {
                throw new SemanticVersionBuilderException("Dotted identifier is not valid");
            }

            return identifierEnumerable is string[] array ? array : identifierEnumerable.ToArray();
        }

        internal static IReadOnlyList<string> ParseDotSplittedIdentifier(
            SemanticVersionPart versionPart,
            IDottedIdentifierParser dottedIdentifierParser,
            IReadOnlyCollection<string>? newDotSplittedIdentifiers) =>
            ParseDottedIdentifier(versionPart, dottedIdentifierParser, CombineDotSplitted(newDotSplittedIdentifiers));

        internal ISemanticVersion BaseVersion { get; }

        private uint? _major, _minor, _patch;
        private IReadOnlyCollection<string>? _preReleaseIdentifiers, _buildIdentifiers;
        private string? _prefix, _preRelease, _build;
        private bool _withPrefix, _withPreRelease, _withBuild;

        /// <summary>
        /// Creates an instance of <see cref="SemanticVersionBuilder"/>.
        /// </summary>
        /// <param name="baseVersion"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SemanticVersionBuilder(ISemanticVersion baseVersion) =>
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

        /// <summary>
        /// Sets the "pre-release"-part of the version.
        /// </summary>
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
        public SemanticVersionBuilder PreRelease(params string[] preReleaseIdentifiers) =>
            PreRelease((IReadOnlyCollection<string>)preReleaseIdentifiers);

        /// <summary>
        /// Unsets the "pre-release"-part of the version.
        /// </summary>
        public SemanticVersionBuilder WithoutPreRelease() =>
            PreRelease(default(string));

        /// <summary>
        /// Sets the "build"-part of the version.
        /// </summary>
        /// <param name="build"></param>
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

        /// <inheritdoc/>
        public SemanticVersionBuilder Build(params string[] buildIdentifiers) =>
            PreRelease((IReadOnlyCollection<string>)buildIdentifiers);

        /// <summary>
        /// Unsets the "build"-part of the version.
        /// </summary>
        public SemanticVersionBuilder WithoutBuild() =>
            Build(default(string));

        /// <summary>
        /// Unsets the "pre-release"-part and the "build"-part of the version. 
        /// </summary>
        public SemanticVersionBuilder WithOnlyCore() =>
            WithoutPreRelease().WithoutBuild();

        private SemanticVersion BuildByParsing(ISemanticVersionContext parser)
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
                SelectVersionNumber(SemanticVersionPart.Major, _major, BaseVersion.Major),
                SelectVersionNumber(SemanticVersionPart.Minor, _minor, BaseVersion.Minor),
                SelectVersionNumber(SemanticVersionPart.Patch, _patch, BaseVersion.Patch),
                SelectDottedIdentifiers(SemanticVersionPart.PreRelease, parser.PreReleaseParser, _withPreRelease, BaseVersion.PreReleaseIdentifiers, _preRelease, _preReleaseIdentifiers),
                SelectDottedIdentifiers(SemanticVersionPart.Build, parser.BuildParser, _withBuild, BaseVersion.BuildIdentifiers, _build, _buildIdentifiers));

            uint SelectVersionNumber(SemanticVersionPart versionPart, uint? newVersionNumber, uint baseVersionNumber)
            {
                if (!newVersionNumber.HasValue) {
                    return baseVersionNumber;
                }

                return ParseVersionNumber(parser.VersionParser, versionPart, newVersionNumber.Value);
            }

            IReadOnlyList<string> SelectDottedIdentifiers(
                SemanticVersionPart versionPart,
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
                    return ParseDottedIdentifier(versionPart, dottedIdentifierParser, newDottedIdentifier);
                }

                return ParseDotSplittedIdentifier(versionPart, dottedIdentifierParser, newDotSplittedIdentifiers);
            }
        }

        /// <summary>
        /// Builds the version.
        /// </summary>
        /// <param name="parser"></param>
        public SemanticVersion ToVersion(ISemanticVersionContext parser) =>
            BuildByParsing(parser);

        /// <summary>
        /// Builds the version. If it does implement <see cref="ISemanticVersionContextProvider"/>
        /// its parser will be taken, otherwise <see cref="SemanticVersionParser.Strict"/>.
        /// </summary>
        public SemanticVersion ToVersion() =>
            ToVersion(BaseVersion.GetContextOrStrict());

        /// <inheritdoc/>
        public static implicit operator SemanticVersion(SemanticVersionBuilder builder) =>
            builder.ToVersion();
    }
}
