using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions.MessageIndicators;
using Vernuntii.MessageConventions;
using Vernuntii.VersionIncrementFlows;
using Vernuntii.VersionIncrementing;

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

        /// <summary>
        /// Continuous delivery preset consisting of
        /// <br/> - <see cref="FalsyMessageIndicator"/> for major and minor,
        /// <br/> - <see cref="TruthyMessageIndicator"/> for patch and
        /// <br/> - <see cref="VersionIncrementMode.Consecutive"/>
        /// </summary>
        public readonly static VersioningPreset ContinuousDelivery = new VersioningPreset() {
            IncrementMode = VersionIncrementMode.Consecutive,
            MessageConvention = new MessageConvention() {
                MajorIndicators = new[] { FalsyMessageIndicator.Default },
                MinorIndicators = new[] { FalsyMessageIndicator.Default },
                PatchIndicators = new[] { TruthyMessageIndicator.Default }
            },
            HeightConvention = HeightConventions.HeightConvention.InPreReleaseAfterFirstDot
        };

        /// <summary>
        /// Default reset. Equivalent to <see cref="ContinuousDelivery"/>.
        /// </summary>
        public readonly static VersioningPreset Default = ContinuousDelivery;

        /// <summary>
        /// Continuous deployment preset consisting of
        /// <br/> - <see cref="FalsyMessageIndicator"/> for major and minor,
        /// <br/> - <see cref="TruthyMessageIndicator"/> for patch and
        /// <br/> - <see cref="VersionIncrementMode.Successive"/>
        /// </summary>
        public readonly static VersioningPreset ContinuousDeployment = new VersioningPreset() {
            IncrementMode = VersionIncrementMode.Successive,
            MessageConvention = new MessageConvention() {
                MajorIndicators = new[] { FalsyMessageIndicator.Default },
                MinorIndicators = new[] { FalsyMessageIndicator.Default },
                PatchIndicators = new[] { TruthyMessageIndicator.Default }
            },
            HeightConvention = HeightConventions.HeightConvention.InPreReleaseAfterFirstDot
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
            HeightConvention = HeightConventions.HeightConvention.InPreReleaseAfterFirstDot
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
            HeightConvention = HeightConventions.HeightConvention.InPreReleaseAfterFirstDot
        };

        /// <inheritdoc/>
        public VersionIncrementMode IncrementMode { get; init; } = VersionIncrementMode.None;

        /// <inheritdoc/>
        public IVersionIncrementFlow IncrementFlow {
            get => _incrementFlow;
            init => _incrementFlow = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        public IMessageConvention MessageConvention {
            get => _messageConvention;
            init => _messageConvention = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        public IHeightConvention HeightConvention {
            get => _heightConvention;
            init => _heightConvention = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IVersionIncrementFlow _incrementFlow = VersionIncrementFlow.None;
        private IMessageConvention _messageConvention = MessageConventions.MessageConvention.None;
        private IHeightConvention _heightConvention = HeightConventions.HeightConvention.None;

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
            IncrementFlow = versioningPreset.IncrementFlow;
            MessageConvention = versioningPreset.MessageConvention;
            HeightConvention = versioningPreset.HeightConvention;
        }

        /// <inheritdoc/>
        public bool Equals(IVersioningPreset? other) =>
            other is not null
            && Equals(IncrementMode, other.IncrementMode)
            && Equals(IncrementFlow, other.IncrementFlow)
            && Equals(MessageConvention, other.MessageConvention)
            && Equals(HeightConvention, other.HeightConvention);

        /// <inheritdoc/>
        public bool Equals(VersioningPreset? other) =>
            Equals((IVersioningPreset?)other);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(IncrementMode);
            hashCode.Add(IncrementFlow);
            hashCode.Add(MessageConvention);
            hashCode.Add(HeightConvention);
            return hashCode.ToHashCode();
        }
    }
}
