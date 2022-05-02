using Vernuntii.HeightConventions;
using Vernuntii.HeightConventions.Rules;
using Vernuntii.MessageVersioning;
using Vernuntii.MessageConventions.MessageIndicators;
using Vernuntii.MessageConventions;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Options class for <see cref="VersionIncrementBuilder"/>.
    /// </summary>
    public sealed record class VersioningPreset : IEquatable<VersioningPreset>, IVersioningPreset
    {
        /// <summary>
        /// Manual preset consisting of
        /// <br/> - none message indicators and
        /// <br/> - <see cref="VersionIncrementMode.None"/>
        /// </summary>
        public readonly static VersioningPreset Manual = new VersioningPreset();

        private readonly static HeightConvention OneDottedPreReleaseHeightConvention = new HeightConvention(HeightIdentifierPosition.PreRelease) {
            Rules = HeightRuleDictionary.BehindFirstDotRules
        };

        /// <summary>
        /// Continous delivery preset consisting of
        /// <br/> - <see cref="FalsyMessageIndicator"/> for major and minor,
        /// <br/> - <see cref="TruthyMessageIndicator"/> for patch and
        /// <br/> - <see cref="VersionIncrementMode.Consecutive"/>
        /// </summary>
        public readonly static VersioningPreset ContinousDelivery = new VersioningPreset() {
            IncrementMode = VersionIncrementMode.Consecutive,
            MessageConvention = new MessageConvention() {
                MajorIndicators = new[] { FalsyMessageIndicator.Default },
                MinorIndicators = new[] { FalsyMessageIndicator.Default },
                PatchIndicators = new[] { TruthyMessageIndicator.Default }
            },
            HeightConvention = OneDottedPreReleaseHeightConvention
        };

        /// <summary>
        /// Default reset. Equivalent to <see cref="ContinousDelivery"/>.
        /// </summary>
        public readonly static VersioningPreset Default = ContinousDelivery;

        /// <summary>
        /// Continous deployment preset consisting of
        /// <br/> - <see cref="FalsyMessageIndicator"/> for major and minor,
        /// <br/> - <see cref="TruthyMessageIndicator"/> for patch and
        /// <br/> - <see cref="VersionIncrementMode.Successive"/>
        /// </summary>
        public readonly static VersioningPreset ContinousDeployment = new VersioningPreset() {
            IncrementMode = VersionIncrementMode.Successive,
            MessageConvention = new MessageConvention() {
                MajorIndicators = new[] { FalsyMessageIndicator.Default },
                MinorIndicators = new[] { FalsyMessageIndicator.Default },
                PatchIndicators = new[] { TruthyMessageIndicator.Default }
            },
            HeightConvention = OneDottedPreReleaseHeightConvention
        };

        /// <summary>
        /// Conventional commits preset consisting of
        /// <br/> - <see cref="ConventionalCommitsMessageIndicator"/> for version core and
        /// <br/> - <see cref="VersionIncrementMode.Consecutive"/>
        /// </summary>
        public readonly static VersioningPreset ConventionalCommitsDelivery = new VersioningPreset() {
            IncrementMode = VersionIncrementMode.Consecutive,
            MessageConvention = new MessageConvention() {
                MajorIndicators = new[] { ConventionalCommitsMessageIndicator.Default },
                MinorIndicators = new[] { ConventionalCommitsMessageIndicator.Default },
                PatchIndicators = new[] { ConventionalCommitsMessageIndicator.Default }
            },
            HeightConvention = OneDottedPreReleaseHeightConvention
        };

        /// <summary>
        /// Conventional commits preset consisting of
        /// <br/> - <see cref="ConventionalCommitsMessageIndicator"/> for version core and
        /// <br/> - <see cref="VersionIncrementMode.Successive"/>
        /// </summary>
        public readonly static VersioningPreset ConventionalCommitsDeployment = new VersioningPreset() {
            IncrementMode = VersionIncrementMode.Successive,
            MessageConvention = new MessageConvention() {
                MajorIndicators = new[] { ConventionalCommitsMessageIndicator.Default },
                MinorIndicators = new[] { ConventionalCommitsMessageIndicator.Default },
                PatchIndicators = new[] { ConventionalCommitsMessageIndicator.Default }
            },
            HeightConvention = OneDottedPreReleaseHeightConvention
        };

        /// <inheritdoc/>
        public VersionIncrementMode IncrementMode { get; init; }

        /// <inheritdoc/>
        public bool RightShiftWhenZeroMajor { get; init; }

        /// <inheritdoc/>
        public IMessageConvention? MessageConvention { get; init; }

        /// <inheritdoc/>
        public IHeightConvention? HeightConvention { get; init; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public VersioningPreset()
        {
        }

        /// <summary>
        /// Creates an copy of <paramref name="versioningPreset"/>.
        /// </summary>
        /// <param name="versioningPreset"></param>
        public VersioningPreset(IVersioningPreset versioningPreset)
        {
            IncrementMode = versioningPreset.IncrementMode;
            MessageConvention = versioningPreset.MessageConvention;
            HeightConvention = versioningPreset.HeightConvention;
        }

        /// <inheritdoc/>
        public bool Equals(VersioningPreset? other) =>
            other is not null
            && Equals(IncrementMode, other.IncrementMode)
            && Equals(MessageConvention, other.MessageConvention);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(IncrementMode);
            hashCode.Add(MessageConvention);
            return hashCode.ToHashCode();
        }
    }
}
