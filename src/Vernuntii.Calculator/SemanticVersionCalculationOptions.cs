using System.Diagnostics.CodeAnalysis;
using Vernuntii.HeightConventions;
using Vernuntii.HeightConventions.Transformation;
using Vernuntii.MessagesProviders;
using Vernuntii.SemVer;
using Vernuntii.VersioningPresets;
using Vernuntii.VersionTransformers;

namespace Vernuntii
{
    /// <summary>
    /// The options class for the next version calculation.
    /// </summary>
    public sealed class SemanticVersionCalculationOptions
    {
        private static ISemanticVersion GetNonNullVersionOrThrow(ISemanticVersion? version) =>
            version ?? throw new ArgumentNullException(nameof(version));

        /// <summary>
        /// The start version (default is 0.1.0). All upcoming transformations are applied on this version.
        /// </summary>
        public ISemanticVersion StartVersion {
            get => _startVersion;
            set => _startVersion = GetNonNullVersionOrThrow(value);
        }

        /// <summary>
        /// A boolean indicating whether the version core of <see cref="StartVersion"/> has been already released.
        /// If true a version core increment is strived.
        /// </summary>
        public bool StartVersionCoreAlreadyReleased { get; set; }

        /// <summary>
        /// The message provider.
        /// </summary>
        public IMessagesProvider? MessagesProvider { get; set; }

        /// <summary>
        /// The version core options.
        /// </summary>
        public IVersioningPreset VersioningPreset {
            get => _versionIncrementOptions;
            set => _versionIncrementOptions = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// A factory to create in instance of <see cref="HeightConventionTransformer"/>.
        /// </summary>
        public Func<IHeightConvention, HeightConventionTransformer>? HeightIdentifierTransformerFactory { private get; init; }

        /// <summary>
        /// Used on calculated version.
        /// </summary>
        public IPreReleaseTransformer? PostTransformer { get; set; }

        /// <summary>
        /// Indicates whether <see cref="PostTransformer"/> is not null and is capable to tranform.
        /// </summary>
        [MemberNotNullWhen(true, nameof(PostTransformer))]
        public bool IsPostTransformerUsable => PostTransformer.CanTransform();

        /// <summary>
        /// Look to the future whether the final version is about to be a release or a pre-release version.
        /// </summary>
        internal bool IsPostVersionPreRelease => !string.IsNullOrEmpty(PostTransformer?.ProspectivePreRelease ?? StartVersion.PreRelease);

        private ISemanticVersion _startVersion = SemanticVersion.OneMinor.With.PreRelease("alpha").ToVersion();
        private IVersioningPreset _versionIncrementOptions = VersioningPresets.VersioningPreset.Default;

        internal HeightConventionTransformer CreateHeightIdentifierTransformer()
        {
            var heightConvention = VersioningPreset.HeightConvention ?? HeightConvention.None;

            return HeightIdentifierTransformerFactory?.Invoke(heightConvention)
                ?? new HeightConventionTransformer(heightConvention, HeightPlaceholderParser.Default);
        }
    }
}
