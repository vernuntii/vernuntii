using Vernuntii.MessageVersioning.MessageIndicators;
using Vernuntii.HeightVersioning;
using Vernuntii.HeightVersioning.Ruling;
using Vernuntii.HeightVersioning.Transforming;

namespace Vernuntii.MessageVersioning
{
    /// <summary>
    /// Options class for <see cref="VersionIncrementBuilder"/>.
    /// </summary>
    public sealed record class VersionIncrementBuilderOptions : IEquatable<VersionIncrementBuilderOptions>
    {
        /// <summary>
        /// Manual preset consisting of
        /// <br/> - none message indicators and
        /// <br/> - <see cref="VersionIncrementMode.None"/>
        /// </summary>
        public readonly static VersionIncrementBuilderOptions Manual = new VersionIncrementBuilderOptions() {
            IncrementMode = VersionIncrementMode.None
        };

        private readonly static HeightConvention OneDottedPreReleaseHeightConvention = new HeightConvention(HeightIdentifierPosition.PreRelease) {
            Rules = HeightRuleDictionary.BehindFirstDotRules
        };

        /// <summary>
        /// Continous delivery preset consisting of
        /// <br/> - <see cref="FalsyMessageIndicator"/> for major and minor,
        /// <br/> - <see cref="TruthyMessageIndicator"/> for patch and
        /// <br/> - <see cref="VersionIncrementMode.Consecutive"/>
        /// </summary>
        public readonly static VersionIncrementBuilderOptions ContinousDelivery = new VersionIncrementBuilderOptions() {
            IncrementMode = VersionIncrementMode.Consecutive,
            MajorIndicators = new[] { FalsyMessageIndicator.Default },
            MinorIndicators = new[] { FalsyMessageIndicator.Default },
            PatchIndicators = new[] { TruthyMessageIndicator.Default },
            HeightConvention = OneDottedPreReleaseHeightConvention
        };

        /// <summary>
        /// Default reset. Equivalent to <see cref="ContinousDelivery"/>.
        /// </summary>
        public readonly static VersionIncrementBuilderOptions Default = ContinousDelivery;

        /// <summary>
        /// Continous deployment preset consisting of
        /// <br/> - <see cref="FalsyMessageIndicator"/> for major and minor,
        /// <br/> - <see cref="TruthyMessageIndicator"/> for patch and
        /// <br/> - <see cref="VersionIncrementMode.Successive"/>
        /// </summary>
        public readonly static VersionIncrementBuilderOptions ContinousDeployment = new VersionIncrementBuilderOptions() {
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
        public readonly static VersionIncrementBuilderOptions ConventionalCommitsDelivery = new VersionIncrementBuilderOptions() {
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
        public readonly static VersionIncrementBuilderOptions ConventionalCommitsDeployment = new VersionIncrementBuilderOptions() {
            IncrementMode = VersionIncrementMode.Successive,
            MajorIndicators = new[] { ConventionalCommitsMessageIndicator.Default },
            MinorIndicators = new[] { ConventionalCommitsMessageIndicator.Default },
            PatchIndicators = new[] { ConventionalCommitsMessageIndicator.Default },
            HeightConvention = OneDottedPreReleaseHeightConvention
        };

        private static Dictionary<string, VersionIncrementBuilderOptions> Presets =
            new Dictionary<string, VersionIncrementBuilderOptions>(StringComparer.OrdinalIgnoreCase) {
                { "", Default },
                { VersioningModePreset.Manual.ToString(), Manual },
                { VersioningModePreset.ContinousDelivery.ToString(), ContinousDelivery },
                { VersioningModePreset.ContinousDeployment.ToString(), ContinousDeployment },
                { VersioningModePreset.ConventionalCommitsDelivery.ToString(), ConventionalCommitsDelivery },
                { VersioningModePreset.ConventionalCommitsDeployment.ToString(), ConventionalCommitsDeployment }
            };

        private static Dictionary<string, VersionIncrementBuilderOptions> Conventions =
            new Dictionary<string, VersionIncrementBuilderOptions>(Presets, StringComparer.OrdinalIgnoreCase) {
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
        private static VersionIncrementBuilderOptions GetFromName(
            IReadOnlyDictionary<string, VersionIncrementBuilderOptions> dictionary,
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
        public static VersionIncrementBuilderOptions GetPreset(string? presetName) =>
            GetFromName(Presets, presetName, nameof(presetName));

        /// <summary>
        /// Creates options from preset name.
        /// </summary>
        /// <param name="preset"></param>
        /// <exception cref="ArgumentException"></exception>
        public static VersionIncrementBuilderOptions GetPreset(VersioningModePreset? preset) =>
            GetFromName(Presets, preset.ToString(), nameof(preset));

        /// <summary>
        /// Creates options from convention name.
        /// </summary>
        /// <param name="messageConventionName"></param>
        /// <exception cref="ArgumentException"></exception>
        public static VersionIncrementBuilderOptions GetMessageConvention(string? messageConventionName) =>
            GetFromName(Conventions, messageConventionName, nameof(messageConventionName));

        /// <summary>
        /// Creates options from convention.
        /// </summary>
        /// <param name="messageConvention"></param>
        /// <exception cref="ArgumentException"></exception>
        public static VersionIncrementBuilderOptions GetMessageConvention(VersioningModePreset? messageConvention) =>
            GetFromName(Conventions, messageConvention.ToString(), nameof(messageConvention));

        /// <summary>
        /// Creates options from convention.
        /// </summary>
        /// <param name="messageConvention"></param>
        /// <exception cref="ArgumentException"></exception>
        public static VersionIncrementBuilderOptions GetMessageConvention(VersioningModeMessageConvention? messageConvention) =>
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
        /// Height convention.
        /// </summary>
        public IHeightConvention? HeightConvention { get; init; }

        /// <summary>
        /// The height identifier transformer.
        /// </summary>
        public HeightConventionTransformer HeightIdentifierTransformer {
            get {
                if (HeightConvention == null) {
                    throw new InvalidOperationException("Height convention is not defined");
                }

                if (_heightIdentifierTransformer is not null) {
                    return _heightIdentifierTransformer;
                }

                HeightConventionTransformer transformer;

                if (HeightIdentifierTransformerFactory != null) {
                    transformer = HeightIdentifierTransformerFactory(HeightConvention);
                } else {
                    transformer = new HeightConventionTransformer(HeightConvention, HeightPlaceholderParser.Default);
                }

                return _heightIdentifierTransformer = transformer;
            }

            init => _heightIdentifierTransformer = value;
        }

        /// <summary>
        /// A factory to create in instance for <see cref="HeightIdentifierTransformer"/> if it is null. Whenever used once.
        /// </summary>
        public Func<IHeightConvention, HeightConventionTransformer>? HeightIdentifierTransformerFactory { private get; init; }

        private HeightConventionTransformer? _heightIdentifierTransformer;

        /// <summary>
        /// Creates an copy of this instance with new convention.
        /// </summary>
        /// <param name="convention"></param>
        /// <returns>New instance.</returns>
        public VersionIncrementBuilderOptions WithConvention(VersionIncrementBuilderOptions convention)
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
        public VersionIncrementBuilderOptions WithConvention(string conventionName) =>
            WithConvention(GetMessageConvention(conventionName));

        /// <summary>
        /// Creates an copy of this instance with new convention.
        /// </summary>
        /// <param name="convention"></param>
        /// <returns>New instance.</returns>
        public VersionIncrementBuilderOptions WithConvention(VersioningModePreset convention) =>
            WithConvention(GetMessageConvention(convention));

        /// <summary>
        /// Creates an copy of this instance with new convention.
        /// </summary>
        /// <param name="convention"></param>
        /// <returns>New instance.</returns>
        public VersionIncrementBuilderOptions WithConvention(VersioningModeMessageConvention convention) =>
            WithConvention(GetMessageConvention(convention));

        /// <inheritdoc/>
        public bool Equals(VersionIncrementBuilderOptions? other) =>
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
