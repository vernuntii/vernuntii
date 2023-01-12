using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer.Parser;
using static Vernuntii.SemVer.SemanticVersionBuilder;

namespace Vernuntii.SemVer
{
    /// <summary>
    /// A strict semantic version implementation.
    /// </summary>
    public record SemanticVersion : ISemanticVersion, ISemanticVersionContextProvider, IComparable<SemanticVersion>
    {
        internal static readonly string[] s_emptyIdentifiers = Array.Empty<string>();

        internal static string CombineDotSplitted(IEnumerable<string>? values) =>
            values is null
            ? string.Empty
            : string.Join('.', values);

        private static ISemanticVersionContext GetContextFromKind(SemanticVersionParserContextKind contextKind) => contextKind switch {
            SemanticVersionParserContextKind.Strict => SemanticVersionContext.Strict,
            SemanticVersionParserContextKind.Erase => SemanticVersionContext.Erase,
            _ => throw new NotSupportedException($"The parser kind \"{contextKind}\" is not supported")
        };

        /// <summary>
        /// Tries to parse a version string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="context"></param>
        /// <param name="version"></param>
        public static bool TryParse(string value, ISemanticVersionContext context, [NotNullWhen(true)] out SemanticVersion? version)
        {
            if (context.Parser.TryParse(
                value,
                out var prefix,
                out var major,
                out var minor,
                out var patch,
                out var preReleaseIdentifiers,
                out var buildIdentifiers)) {
                version = new SemanticVersion(context, prefix, major.Value, minor.Value, patch.Value, preReleaseIdentifiers?.ToArray(), buildIdentifiers?.ToArray());
                return true;
            }

            version = null;
            return false;
        }

        /// <summary>
        /// Tries to parse a version string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="context"></param>
        /// <param name="version"></param>
        public static bool TryParse(string value, SemanticVersionParserContextKind context, [NotNullWhen(true)] out SemanticVersion? version) =>
            TryParse(value, GetContextFromKind(context), out version);

        /// <summary>
        /// Tries to parse a version string with <see cref="SemanticVersionContext.Strict"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="version"></param>
        public static bool TryParse(string value, [NotNullWhen(true)] out SemanticVersion? version) =>
            TryParse(value, SemanticVersionContext.Strict, out version);

        /// <summary>
        /// Parses a version string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="context"></param>
        /// <exception cref="ArgumentException"></exception>
        public static SemanticVersion Parse(string value, ISemanticVersionContext context)
        {
            if (!TryParse(value, context, out var result)) {
                throw new ArgumentException($"Version string is not SemVer-compatible: {value}");
            }

            return result;
        }

        /// <summary>
        /// Parses a version string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="contextKind"></param>
        /// <exception cref="ArgumentException"></exception>
        public static SemanticVersion Parse(string value, SemanticVersionParserContextKind contextKind) =>
            Parse(value, GetContextFromKind(contextKind));

        /// <summary>
        /// Parses a version string with <see cref="SemanticVersionContext.Strict"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException"></exception>
        public static SemanticVersion Parse(string value) =>
            Parse(value, SemanticVersionContext.Strict);

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

        /// <inheritdoc/>
        public string Prefix {
            get => _prefix;

            init {
                ValidatePrefix(Context.PrefixValidator, value);
                _prefix = value;
            }
        }

        /// <inheritdoc/>
        public bool HasPrefix => Prefix.Length != 0;

        /// <inheritdoc/>
        public uint Major {
            get => _major;
            init => _major = ParseVersionNumber(Context.VersionParser, SemanticVersionPart.Major, value);
        }

        /// <inheritdoc/>
        public uint Minor {
            get => _minor;
            init => _minor = ParseVersionNumber(Context.VersionParser, SemanticVersionPart.Minor, value);
        }

        /// <inheritdoc/>
        public uint Patch {
            get => _patch;
            init => _patch = ParseVersionNumber(Context.VersionParser, SemanticVersionPart.Patch, value);
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> PreReleaseIdentifiers {
            get => _preReleaseIdentifiers;

            init {
                _preRelease = null;
                _preReleaseIdentifiers = ParseDotSplittedIdentifier(SemanticVersionPart.PreRelease, Context.PreReleaseParser, value);
            }
        }

        /// <inheritdoc/>
        public string PreRelease {
            get => _preRelease ??= CombineDotSplitted(PreReleaseIdentifiers);

            init {
                _preRelease = null;
                _preReleaseIdentifiers = ParseDottedIdentifier(SemanticVersionPart.PreRelease, Context.PreReleaseParser, value);
            }
        }

        /// <inheritdoc/>
        public bool IsPreRelease => PreReleaseIdentifiers.Count != 0;

        /// <inheritdoc/>
        public IReadOnlyList<string> BuildIdentifiers {
            get => _buildIdentifiers;

            init {
                _build = null;
                _buildIdentifiers = ParseDotSplittedIdentifier(SemanticVersionPart.Build, Context.BuildParser, value);
            }
        }

        /// <inheritdoc/>
        public string Build {
            get => _build ??= CombineDotSplitted(BuildIdentifiers);

            init {
                _build = null;
                _buildIdentifiers = ParseDottedIdentifier(SemanticVersionPart.Build, Context.BuildParser, value);
            }
        }

        /// <inheritdoc/>
        public bool HasBuild => BuildIdentifiers.Count != 0;

        /// <summary>
        /// Builder for creating a copy of this instance.
        /// </summary>
        public SemanticVersionBuilder With => new(this);

        /// <summary>
        /// This parser was used at creation of this instance and it gets inherited
        /// when you create a shallow copy of this instance for example by using
        /// <see cref="With"/>.
        /// </summary>
        public ISemanticVersionContext Context { get; }

        private string _prefix;
        private uint _major;
        private uint _minor;
        private uint _patch;
        private string? _preRelease;
        private IReadOnlyList<string> _preReleaseIdentifiers;
        private string? _build;
        private IReadOnlyList<string> _buildIdentifiers;

        /// <summary>
        /// Creates a semantic version holding a reference to strict parser.
        /// </summary>
        public SemanticVersion()
        {
            Context = SemanticVersionContext.Strict;
            _prefix = string.Empty;
            _preReleaseIdentifiers = s_emptyIdentifiers;
            _buildIdentifiers = s_emptyIdentifiers;
        }

        /// <summary>
        /// Copies the instance of <paramref name="semanticVersion"/>.
        /// </summary>
        /// <param name="semanticVersion"></param>
        public SemanticVersion(ISemanticVersion semanticVersion)
        {
            Context = semanticVersion.GetContextOrStrict();
            _prefix = semanticVersion.Prefix;
            _major = semanticVersion.Major;
            _minor = semanticVersion.Minor;
            _patch = semanticVersion.Patch;
            _preRelease = semanticVersion.PreRelease;
            _preReleaseIdentifiers = semanticVersion.PreReleaseIdentifiers;
            _build = semanticVersion.Build;
            _buildIdentifiers = semanticVersion.BuildIdentifiers;
        }

        /// <summary>
        /// Creates a semantic version.
        /// </summary>
        public SemanticVersion(ISemanticVersionContext parser)
        {
            Context = parser ?? throw new ArgumentNullException(nameof(parser));
            _prefix = string.Empty;
            _preReleaseIdentifiers = s_emptyIdentifiers;
            _buildIdentifiers = s_emptyIdentifiers;
        }

        /// <summary>
        /// Creates
        /// </summary>
        /// <param name="context"></param>
        /// <param name="prefix"></param>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="patch"></param>
        /// <param name="preReleaseIdentifiers"></param>
        /// <param name="buildIdentifiers"></param>
        internal SemanticVersion(
            ISemanticVersionContext context,
            string? prefix,
            uint major,
            uint minor,
            uint patch,
            IReadOnlyList<string>? preReleaseIdentifiers,
            IReadOnlyList<string>? buildIdentifiers)
        {
            Context = context;
            _prefix = prefix ?? string.Empty;
            _major = major;
            _minor = minor;
            _patch = patch;
            _preReleaseIdentifiers = preReleaseIdentifiers ?? s_emptyIdentifiers;
            _buildIdentifiers = buildIdentifiers ?? s_emptyIdentifiers;
        }

        /// <summary>
        /// Compares the two versions.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="comparisonMode"></param>
        /// <returns>-1 if <see langword="this"/> is smaller to <paramref name="other"/>, 0 if equal and 1 if greater.</returns>
        public int CompareTo(ISemanticVersion? other, SemanticVersionComparisonMode comparisonMode) =>
            SemanticVersionComparer.Compare(this, other, comparisonMode);

        /// <summary>
        /// Compares the two versions. Build won't be considered in comparison.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>-1 if <see langword="this"/> is smaller to <paramref name="other"/>, 0 if equal and 1 if greater.</returns>
        public int CompareTo(ISemanticVersion? other) =>
            CompareTo(other, SemanticVersionComparisonMode.VersionRelease);

        /// <summary>
        /// Compares the two versions. Build won't be considered in comparison.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>-1 if <see langword="this"/> is smaller to <paramref name="other"/>, 0 if equal and 1 if greater.</returns>
        public int CompareTo(SemanticVersion? other) =>
            CompareTo(other, SemanticVersionComparisonMode.VersionRelease);

        /// <summary>
        /// Compares the two versions. Build won't be considered in comparison.
        /// </summary>
        /// <param name="obj"></param>
        public int CompareTo(object? obj) => CompareTo(obj as SemanticVersion);

        /// <summary>
        /// Gets the version string in its full representation.
        /// </summary>
        public override string ToString() => this.Format(SemanticVersionFormat.SemanticVersion);

        /// <inheritdoc/>
        public virtual bool Equals([NotNullWhen(true)] ISemanticVersion? other) =>
            SemanticVersionComparer.VersionReleaseBuild.Equals(this, other);

        /// <inheritdoc/>
        public virtual bool Equals([NotNullWhen(true)] SemanticVersion? other) =>
            Equals((ISemanticVersion?)other);

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
