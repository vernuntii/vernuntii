using Vernuntii.MessageVersioning.HeightConventions;
using Vernuntii.MessageVersioning.MessageIndicators;
using Vernuntii.MessageVersioning.HeightConventions.Ruling;

namespace Vernuntii.MessageVersioning
{
    /// <summary>
    /// Options class for <see cref="VersionTransformerBuilder"/>.
    /// </summary>
    public sealed record class VersionTransformerBuilderOptions : IEquatable<VersionTransformerBuilderOptions>
    {
        /// <summary>
        /// Manual preset consisting of
        /// <br/> - none message indicators and
        /// <br/> - <see cref="VersionIncrementMode.None"/>
        /// </summary>
        public readonly static VersionTransformerBuilderOptions Manual = new VersionTransformerBuilderOptions() {
            IncrementMode = VersionIncrementMode.None
        };

        /// <summary>
        /// Continous delivery preset consisting of
        /// <br/> - <see cref="FalsyMessageIndicator"/> for major and minor,
        /// <br/> - <see cref="TruthyMessageIndicator"/> for patch and
        /// <br/> - <see cref="VersionIncrementMode.Consecutive"/>
        /// </summary>
        public readonly static VersionTransformerBuilderOptions ContinousDelivery = new VersionTransformerBuilderOptions() {
            IncrementMode = VersionIncrementMode.Consecutive,
            MajorIndicators = new[] { FalsyMessageIndicator.Default },
            MinorIndicators = new[] { FalsyMessageIndicator.Default },
            PatchIndicators = new[] { TruthyMessageIndicator.Default },
            HeightConvention = OneDottedPreReleaseHeightConvention
        };

        /// <summary>
        /// Default reset. Equivalent to <see cref="ContinousDelivery"/>.
        /// </summary>
        public readonly static VersionTransformerBuilderOptions Default = ContinousDelivery;

        /// <summary>
        /// Continous deployment preset consisting of
        /// <br/> - <see cref="FalsyMessageIndicator"/> for major and minor,
        /// <br/> - <see cref="TruthyMessageIndicator"/> for patch and
        /// <br/> - <see cref="VersionIncrementMode.Successive"/>
        /// </summary>
        public readonly static VersionTransformerBuilderOptions ContinousDeployment = new VersionTransformerBuilderOptions() {
            IncrementMode = VersionIncrementMode.Successive,
            MajorIndicators = new[] { FalsyMessageIndicator.Default },
            MinorIndicators = new[] { FalsyMessageIndicator.Default },
            PatchIndicators = new[] { TruthyMessageIndicator.Default },
            HeightConvention = OneDottedPreReleaseHeightConvention
        };

        /// <summary>
        /// Conventional commits preset consisting of
        /// <br/> - <see cref="ConventionalCommitsMessageIndicator"/> for version core and
        /// <br/> - <see cref="VersionIncrementMode.Consecutive"/>
        /// </summary>
        public readonly static VersionTransformerBuilderOptions ConventionalCommitsDelivery = new VersionTransformerBuilderOptions() {
            IncrementMode = VersionIncrementMode.Consecutive,
            MajorIndicators = new[] { ConventionalCommitsMessageIndicator.Default },
            MinorIndicators = new[] { ConventionalCommitsMessageIndicator.Default },
            PatchIndicators = new[] { ConventionalCommitsMessageIndicator.Default },
            HeightConvention = OneDottedPreReleaseHeightConvention
        };

        /// <summary>
        /// Conventional commits preset consisting of
        /// <br/> - <see cref="ConventionalCommitsMessageIndicator"/> for version core and
        /// <br/> - <see cref="VersionIncrementMode.Successive"/>
        /// </summary>
        public readonly static VersionTransformerBuilderOptions ConventionalCommitsDeployment = new VersionTransformerBuilderOptions() {
            IncrementMode = VersionIncrementMode.Successive,
            MajorIndicators = new[] { ConventionalCommitsMessageIndicator.Default },
            MinorIndicators = new[] { ConventionalCommitsMessageIndicator.Default },
            PatchIndicators = new[] { ConventionalCommitsMessageIndicator.Default },
            HeightConvention = OneDottedPreReleaseHeightConvention
        };

        private readonly static HeightConvention OneDottedPreReleaseHeightConvention = new HeightConvention(HeightPosition.PreRelease) {
            Rules = HeightRuleDictionary.OneDotRules
        };

        private static Dictionary<string, VersionTransformerBuilderOptions> Presets =
            new Dictionary<string, VersionTransformerBuilderOptions>(StringComparer.OrdinalIgnoreCase) {
                { "", Default },
                { VersioningModePreset.Manual.ToString(), Manual },
                { VersioningModePreset.ContinousDelivery.ToString(), ContinousDelivery },
                { VersioningModePreset.ContinousDeployment.ToString(), ContinousDeployment },
                { VersioningModePreset.ConventionalCommitsDelivery.ToString(), ConventionalCommitsDelivery },
                { VersioningModePreset.ConventionalCommitsDeployment.ToString(), ConventionalCommitsDeployment }
            };

        private static Dictionary<string, VersionTransformerBuilderOptions> Conventions =
            new Dictionary<string, VersionTransformerBuilderOptions>(Presets, StringComparer.OrdinalIgnoreCase) {
                { VersioningModeMessageConvention.Continous.ToString(), ContinousDelivery },
                { VersioningModeMessageConvention.ConventionalCommits.ToString(), ConventionalCommitsDelivery }
            };

        /// <summary>
        /// Creates from name.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="name"></param>
        /// <param name="nameToDisplayOnError">Name to display on error.</param>
        /// <returns>New instance.</returns>
        /// <exception cref="ArgumentException"></exception>
        private static VersionTransformerBuilderOptions GetFromName(
            IReadOnlyDictionary<string, VersionTransformerBuilderOptions> dictionary,
            string? name,
            string nameToDisplayOnError)
        {
            if (name == null) {
                name = string.Empty;
            }

            if (!dictionary.TryGetValue(name, out var options)) {
                throw new ArgumentException($"The {nameToDisplayOnError} \"{name}\" does not exist");
            }

            return options;
        }

        /// <summary>
        /// Creates options from preset name.
        /// </summary>
        /// <param name="presetName"></param>
        /// <exception cref="ArgumentException"></exception>
        public static VersionTransformerBuilderOptions GetPreset(string? presetName) =>
            GetFromName(Presets, presetName, nameof(presetName));

        /// <summary>
        /// Creates options from preset name.
        /// </summary>
        /// <param name="preset"></param>
        /// <exception cref="ArgumentException"></exception>
        public static VersionTransformerBuilderOptions GetPreset(VersioningModePreset? preset) =>
            GetFromName(Presets, preset.ToString(), nameof(preset));

        /// <summary>
        /// Creates options from convention name.
        /// </summary>
        /// <param name="messageConventionName"></param>
        /// <exception cref="ArgumentException"></exception>
        public static VersionTransformerBuilderOptions GetMessageConvention(string? messageConventionName) =>
            GetFromName(Conventions, messageConventionName, nameof(messageConventionName));

        /// <summary>
        /// Creates options from convention.
        /// </summary>
        /// <param name="messageConvention"></param>
        /// <exception cref="ArgumentException"></exception>
        public static VersionTransformerBuilderOptions GetMessageConvention(VersioningModePreset? messageConvention) =>
            GetFromName(Conventions, messageConvention.ToString(), nameof(messageConvention));

        /// <summary>
        /// Creates options from convention.
        /// </summary>
        /// <param name="messageConvention"></param>
        /// <exception cref="ArgumentException"></exception>
        public static VersionTransformerBuilderOptions GetMessageConvention(VersioningModeMessageConvention? messageConvention) =>
            GetFromName(Conventions, messageConvention.ToString(), nameof(messageConvention));

        private static bool Equals(IEnumerable<IMessageIndicator>? x, IEnumerable<IMessageIndicator>? y) =>
            ReferenceEquals(x, y)
            || (x is not null
                && y is not null
                && x.SequenceEqual(y, MessageIndicatorNameComparer.Default));

        private static int GetHashCode(IEnumerable<IMessageIndicator>? enumerable)
        {
            if (enumerable is null) {
                return 0;
            }

            var hashCode = new HashCode();

            foreach (var indicator in enumerable) {
                hashCode.Add(MessageIndicatorNameComparer.Default.GetHashCode(indicator));
            }

            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Increment mode.
        /// </summary>
        public VersionIncrementMode IncrementMode { get; init; }
        /// <summary>
        /// Major indicators.
        /// </summary>
        public IReadOnlyCollection<IMessageIndicator>? MajorIndicators { get; init; }
        /// <summary>
        /// Minor indicators.
        /// </summary>
        public IReadOnlyCollection<IMessageIndicator>? MinorIndicators { get; init; }
        /// <summary>
        /// Patch indicators.
        /// </summary>
        public IReadOnlyCollection<IMessageIndicator>? PatchIndicators { get; init; }
        /// <summary>
        /// Build height rules.
        /// </summary>
        public IHeightConvention? HeightConvention { get; init; }

        /// <summary>
        /// Creates an copy of this instance with new convention.
        /// </summary>
        /// <param name="convention"></param>
        /// <returns>New instance.</returns>
        public VersionTransformerBuilderOptions WithConvention(VersionTransformerBuilderOptions convention)
        {
            return this with {
                MajorIndicators = convention.MajorIndicators,
                MinorIndicators = convention.MinorIndicators,
                PatchIndicators = convention.PatchIndicators,
            };
        }

        /// <summary>
        /// Creates an copy of this instance with new convention.
        /// </summary>
        /// <param name="conventionName"></param>
        /// <returns>New instance.</returns>
        public VersionTransformerBuilderOptions WithConvention(string conventionName) =>
            WithConvention(GetMessageConvention(conventionName));

        /// <summary>
        /// Creates an copy of this instance with new convention.
        /// </summary>
        /// <param name="convention"></param>
        /// <returns>New instance.</returns>
        public VersionTransformerBuilderOptions WithConvention(VersioningModePreset convention) =>
            WithConvention(GetMessageConvention(convention));

        /// <summary>
        /// Creates an copy of this instance with new convention.
        /// </summary>
        /// <param name="convention"></param>
        /// <returns>New instance.</returns>
        public VersionTransformerBuilderOptions WithConvention(VersioningModeMessageConvention convention) =>
            WithConvention(GetMessageConvention(convention));

        /// <inheritdoc/>
        public bool Equals(VersionTransformerBuilderOptions? other) =>
            other is not null
            && Equals(MajorIndicators, other.MajorIndicators)
            && Equals(MinorIndicators, other.MinorIndicators)
            && Equals(PatchIndicators, other.PatchIndicators);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(IncrementMode);
            hashCode.Add(GetHashCode(MajorIndicators));
            hashCode.Add(GetHashCode(MinorIndicators));
            hashCode.Add(GetHashCode(PatchIndicators));
            return hashCode.ToHashCode();
        }
    }
}
