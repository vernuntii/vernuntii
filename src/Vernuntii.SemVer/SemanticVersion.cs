using System.Diagnostics.CodeAnalysis;
using System.Text;
using Vernuntii.SemVer;
using Vernuntii.SemVer.Parser;
using static Vernuntii.SemVer.SemanticVersionBuilder;

namespace Vernuntii
{
    /// <summary>
    /// A strict semantic version implementation.
    /// </summary>
    public record SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        internal static readonly string[] EmptyIdentifiers = Array.Empty<string>();

        private static string CombineDotSplitted(IReadOnlyList<string>? values) =>
            values is null
            ? string.Empty
            : string.Join('.', values);

        /// <summary>
        /// Tries to parse a version string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parser"></param>
        /// <param name="version"></param>
        public static bool TryParse(string value, ISemanticVersionParser parser, [NotNullWhen(true)] out SemanticVersion? version)
        {
            if (parser.TryParse(
                value,
                out var prefix,
                out var major,
                out var minor,
                out var patch,
                out var preReleaseIdentifiers,
                out var buildIdentifiers)) {
                version = new SemanticVersion(parser, prefix, major.Value, minor.Value, patch.Value, preReleaseIdentifiers?.ToArray(), buildIdentifiers?.ToArray());
                return true;
            }

            version = null;
            return false;
        }

        /// <summary>
        /// Tries to parse a version string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="version"></param>
        public static bool TryParse(string value, [NotNullWhen(true)] out SemanticVersion? version) =>
            TryParse(value, SemanticVersionParser.Strict, out version);

        /// <summary>
        /// Parses a version string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parser"></param>
        public static SemanticVersion Parse(string value, ISemanticVersionParser parser)
        {
            if (!TryParse(value, parser, out var result)) {
                throw new ArgumentException($"Version string is not SemVer-compatible: {value}");
            }

            return result;
        }

        /// <summary>
        /// Parses a version string.
        /// </summary>
        /// <param name="value"></param>
        public static SemanticVersion Parse(string value) =>
            Parse(value, SemanticVersionParser.Strict);

        /// <summary>
        /// 0.0.0
        /// </summary>
        public static SemanticVersion Zero { get; } = new SemanticVersion();

        /// <summary>
        /// 1.0.0
        /// </summary>
        public static SemanticVersion OneMajor { get; } = Zero.With.Major(1);

        /// <summary>
        /// 0.1.0
        /// </summary>
        public static SemanticVersion OneMinor { get; } = Zero.With.Minor(1);

        /// <summary>
        /// 0.0.1
        /// </summary>
        public static SemanticVersion OnePatch { get; } = Zero.With.Patch(1);

        /// <summary>
        /// Prefix of the version.
        /// </summary>
        public string Prefix {
            get => _prefix;

            init {
                ValidatePrefix(Parser.PrefixValidator, value);
                _prefix = value;
            }
        }

        /// <summary>
        /// Checks whether a prefix exists.
        /// </summary>
        public bool HasPrefix => Prefix.Length != 0;

        /// <summary>
        /// Major version.
        /// </summary>
        public uint Major {
            get => _major;
            init => _major = ParseVersionNumber(Parser.VersionNumberParser, value);
        }

        /// <summary>
        /// Minor version.
        /// </summary>
        public uint Minor {
            get => _minor;
            init => _minor = ParseVersionNumber(Parser.VersionNumberParser, value);
        }

        /// <summary>
        /// Patch version.
        /// </summary>
        public uint Patch {
            get => _patch;
            init => _patch = ParseVersionNumber(Parser.VersionNumberParser, value);
        }

        /// <summary>
        /// Pre-release identifiers of the version.
        /// </summary>
        public IReadOnlyList<string> PreReleaseIdentifiers {
            get => _preReleaseIdentifiers;

            init {
                _preRelease = null;
                _preReleaseIdentifiers = ParseDotSplittedIdentifier(Parser.PreReleaseParser, value);
            }
        }

        /// <summary>
        /// Dot-separated pre-release of the version.
        /// </summary>
        public string PreRelease {
            get => _preRelease ??= CombineDotSplitted(PreReleaseIdentifiers);

            init {
                _preRelease = null;
                _preReleaseIdentifiers = ParseDottedIdentifier(Parser.PreReleaseParser, value);
            }
        }

        /// <summary>
        /// True if pre-release exists for the version.
        /// </summary>
        public bool IsPreRelease => PreReleaseIdentifiers.Count != 0;

        /// <summary>
        /// Build identifiers of the version.
        /// </summary>
        public IReadOnlyList<string> BuildIdentifiers {
            get => _buildIdentifiers;

            init {
                _build = null;
                _buildIdentifiers = ParseDotSplittedIdentifier(Parser.BuildParser, value);
            }
        }

        /// <summary>
        /// Dot-separated build of the version.
        /// </summary>
        public string Build {
            get => _build ??= CombineDotSplitted(BuildIdentifiers);

            init {
                _build = null;
                _buildIdentifiers = ParseDottedIdentifier(Parser.BuildParser, value);
            }
        }

        /// <summary>
        /// True if build exists for the version.
        /// </summary>
        public bool HasBuild => BuildIdentifiers.Count != 0;

        /// <summary>
        /// Builder for creating a copy of this instance.
        /// </summary>
        public SemanticVersionBuilder With => new SemanticVersionBuilder(this);

        internal ISemanticVersionParser Parser { get; }

        private string _prefix;
        private uint _major;
        private uint _minor;
        private uint _patch;
        private string? _preRelease, _build;
        private IReadOnlyList<string> _buildIdentifiers;
        private IReadOnlyList<string> _preReleaseIdentifiers;

        /// <summary>
        /// Creates a semantic version holding a reference to strict parser.
        /// </summary>
        public SemanticVersion()
        {
            Parser = SemanticVersionParser.Strict;
            _prefix = string.Empty;
            _preReleaseIdentifiers = EmptyIdentifiers;
            _buildIdentifiers = EmptyIdentifiers;
        }

        /// <summary>
        /// Creates a semantic version.
        /// </summary>
        public SemanticVersion(ISemanticVersionParser parser)
        {
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _prefix = string.Empty;
            _preReleaseIdentifiers = EmptyIdentifiers;
            _buildIdentifiers = EmptyIdentifiers;
        }

        /// <summary>
        /// Creates
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="prefix"></param>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="patch"></param>
        /// <param name="preReleaseIdentifiers"></param>
        /// <param name="buildIdentifiers"></param>
        internal SemanticVersion(
            ISemanticVersionParser parser,
            string? prefix,
            uint major,
            uint minor,
            uint patch,
            IReadOnlyList<string>? preReleaseIdentifiers,
            IReadOnlyList<string>? buildIdentifiers)
        {
            Parser = parser;
            _prefix = prefix ?? string.Empty;
            _major = major;
            _minor = minor;
            _patch = patch;
            _preReleaseIdentifiers = preReleaseIdentifiers ?? EmptyIdentifiers;
            _buildIdentifiers = buildIdentifiers ?? EmptyIdentifiers;
        }

        /// <summary>
        /// Compares the two versions. Build won't be considered in comparison.
        /// </summary>
        /// <param name="other"></param>
        public int CompareTo(SemanticVersion? other) =>
            SemanticVersionComparer.Compare(this, other, SemanticVersionComparisonMode.VersionRelease);

        /// <summary>
        /// Compares the two versions. Build won't be considered in comparison.
        /// </summary>
        /// <param name="obj"></param>
        public int CompareTo(object? obj) => CompareTo(obj as SemanticVersion);

        private void AppendMaybePrefix(StringBuilder stringBuilder)
        {
            if (HasPrefix) {
                stringBuilder.Append(Prefix);
            }
        }

        private void AppendVersion(StringBuilder stringBuilder)
        {
            stringBuilder.Append(Major);
            stringBuilder.Append('.');
            stringBuilder.Append(Minor);
            stringBuilder.Append('.');
            stringBuilder.Append(Patch);
        }

        private void AppendMaybePreRelease(StringBuilder stringBuilder)
        {
            if (IsPreRelease) {
                stringBuilder.Append('-');
                stringBuilder.Append(PreRelease);
            }
        }

        private void AppendMaybeBuild(StringBuilder stringBuilder)
        {
            if (HasBuild) {
                stringBuilder.Append('+');
                stringBuilder.Append(Build);
            }
        }

        /// <summary>
        /// Gets the version in a custom format.
        /// </summary>
        /// <param name="format"></param>
        public string ToString(SemanticVersionFormat format)
        {
            var stringBuilder = new StringBuilder();

            if (format.HasFlag(SemanticVersionFormat.Prefix)) {
                AppendMaybePrefix(stringBuilder);
            }

            if (format.HasFlag(SemanticVersionFormat.Version)) {
                AppendVersion(stringBuilder);
            }

            if (format.HasFlag(SemanticVersionFormat.PreRelease)) {
                AppendMaybePreRelease(stringBuilder);
            }

            if (format.HasFlag(SemanticVersionFormat.Build)) {
                AppendMaybeBuild(stringBuilder);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Gets the version string in its full representation.
        /// </summary>
        public override string ToString() => ToString(SemanticVersionFormat.SemanticVersion);

        /// <inheritdoc/>
        public virtual bool Equals(SemanticVersion? other) =>
            SemanticVersionComparer.VersionReleaseBuild.Equals(this, other);

        /// <inheritdoc/>
        public override int GetHashCode() =>
            SemanticVersionComparer.VersionReleaseBuild.GetHashCode(this);

        /// <inheritdoc/>
        public static bool operator <(SemanticVersion left, SemanticVersion right) =>
            ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;

        /// <inheritdoc/>
        public static bool operator <=(SemanticVersion left, SemanticVersion right) =>
            ReferenceEquals(left, null) || left.CompareTo(right) <= 0;

        /// <inheritdoc/>
        public static bool operator >(SemanticVersion left, SemanticVersion right) =>
            !ReferenceEquals(left, null) && left.CompareTo(right) > 0;

        /// <inheritdoc/>
        public static bool operator >=(SemanticVersion left, SemanticVersion right) =>
            ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
}
