using System.Diagnostics.CodeAnalysis;
using Vernuntii.HeightConventions.Transformation;
using Vernuntii.SemVer;
using Vernuntii.VersioningPresets;
using Vernuntii.VersionTransformers;

namespace Vernuntii.VersionIncrementing
{
    /// <summary>
    /// Represents the context for <see cref="VersionIncrementBuilder"/>.
    /// </summary>
    public record VersionIncrementContext
    {
        /// <inheritdoc cref="VersionIncrementationOptions.VersioningPreset"/>
        public IVersioningPreset VersioningPreset => _incremenationOptions.VersioningPreset;

        /// <inheritdoc cref="VersionIncrementationOptions.StartVersion"/>
        public ISemanticVersion StartVersion => _incremenationOptions.StartVersion;

        /// <inheritdoc cref="VersionIncrementationOptions.IsPostVersionPreRelease"/>
        public bool IsPostVersionPreRelease => _incremenationOptions.IsPostVersionPreRelease;

        /// <inheritdoc cref="VersionIncrementationOptions.IsStartVersionCoreAlreadyReleased"/>
        public bool IsStartVersionCoreAlreadyReleased => _incremenationOptions.IsStartVersionCoreAlreadyReleased;

        /// <summary>
        /// The transformer used for changing the pre-release to a desired one.
        /// </summary>
        public IPreReleaseTransformer? PostVersionPreReleaseTransformer => _incremenationOptions.PostTransformer;

        /// <summary>
        /// If <see langword="true"/>, then it indicates, that the start version will be changing from pre-release
        /// to release or vice versa, or the non-null pre-release is changing to a different non-null pre-release.
        /// </summary>
        [MemberNotNullWhen(true, nameof(PostVersionPreReleaseTransformer))]
        public bool IsStartVersionPreReleaseAlternating { get; }

        /// <summary>
        /// The current version before the next transformation is applied.
        /// </summary>
        public ISemanticVersion CurrentVersion {
            get => _currentVersion ?? _incremenationOptions.StartVersion;

            init {
                _currentVersion = value;
                ResetCurrentVersionDerivedInformations();
            }
        }

        /// <summary>
        /// A boolean indicating whether the current version contains
        /// at least one major increment towards the initial version.
        /// </summary>
        public bool DoesCurrentVersionContainsMajorIncrement =>
            _doesCurrentVersionContainsMajorIncrement ??= CurrentVersionContainsMajorIncrement();

        /// <summary>
        /// A boolean indicating whether the current version contains
        /// at least one minor increment towards the initial version.
        /// </summary>
        public bool DoesCurrentVersionContainsMinorIncrement =>
            _doesCurrentVersionContainsMinorIncrement ??= CurrentVersionContainsMinorIncrement();

        /// <summary>
        /// A boolean indicating whether the current version contains
        /// at least one patch increment towards the initial version.
        /// </summary>
        public bool DoesCurrentVersionContainsPatchIncrement =>
            _doesCurrentVersionContainsPatchIncrement ??= CurrentVersionContainsPatchIncrement();

        /// <summary>
        /// An boolean (implicit) indicating whether the current version contains
        /// at least one height increment towards the initial version.
        /// </summary>
        public VersionHeightInformations DoesCurrentVersionContainsHeightIncrement =>
            _doesCurrentVersionContainsHeightIncrement ??= CurrentVersionContainsHeightIncrement();

        /// <summary>
        /// Current version is equivalent to "major.0.0".
        /// </summary>
        public bool IsRightSideOfMajorOfCurrentVersionCoreZeroed =>
            CurrentVersion.Minor == 0 && CurrentVersion.Patch == 0;

        /// <summary>
        /// Current version is equivalent to "major.minor.0".
        /// </summary>
        public bool IsRightSideOfMinorOfCurrentVersionCoreZeroed =>
            CurrentVersion.Patch == 0;

        /// <summary>
        /// The height identifier transformer.
        /// </summary>
        public HeightConventionTransformer HeightIdentifierTransformer { get; }

        /// <summary>
        /// <see langword="true"/> if major of current version is zero and versioning preset allows right shifting.
        /// </summary>
        public bool CanFlowDownstreamMajor => _canFlowDownstreamMajor ??=
            _incremenationOptions.VersioningPreset.IncrementFlow.Condition == VersionIncrementFlows.VersionIncrementFlowCondition.ZeroMajor
            && _incremenationOptions.StartVersion.Major == 0
            && _incremenationOptions.VersioningPreset.IncrementFlow.MajorFlow == VersionIncrementFlows.VersionIncrementFlowMode.Downstream;

        /// <summary>
        /// <see langword="true"/> if minor of current version is zero and versioning preset allows right shifting.
        /// </summary>
        public bool CanFlowDownstreamMinor => _canFlowDownstreamMinor ??=
            _incremenationOptions.VersioningPreset.IncrementFlow.Condition == VersionIncrementFlows.VersionIncrementFlowCondition.ZeroMajor
            && _incremenationOptions.StartVersion.Major == 0
            && _incremenationOptions.VersioningPreset.IncrementFlow.MinorFlow == VersionIncrementFlows.VersionIncrementFlowMode.Downstream;

        /// <summary>
        /// <see langword="true"/> if indicator of next version has been right shifted. For debug and test puposes.
        /// </summary>
        internal bool IsVersionDownstreamFlowed { get; set; }

        /// <summary>
        /// <see langword="null"/> means, pre-release is not adaptable.
        /// </summary>
        internal bool? IsPreReleaseOfCurrentVersionAdapted =>
            GetOrCalculateWhetherPreReleaseOfCurrentVersionIsAdapted();

        [MemberNotNullWhen(true, nameof(PostVersionPreReleaseTransformer))]
        internal bool IsPreReleaseAdaptionOfCurrentVersionRequired =>
            IsPreReleaseOfCurrentVersionAdapted == false;

        /// <summary>
        /// <see langword="null"/> means, pre-release is not adaptable.
        /// </summary>
        internal bool? IsPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted =>
            GetOrCalculateWhetherPreReleaseOfStartVersionCoreEquivalentCurrentVersionIsAdapted();

        [MemberNotNullWhen(true, nameof(PostVersionPreReleaseTransformer))]
        internal bool IsPreReleaseAdaptionOfStartVersionCoreEquivalentCurrentVersionRequired =>
            IsPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted == false;

        private bool? IsCurrentVersionEquivalentToStartVersionAfterPreReleaseAdaption => IsPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted == true
            ? SemanticVersionComparer.Version.Equals(CurrentVersion, StartVersion)
            : null; // Won't be ever release;

        /// <summary>
        /// The options of on-going calculation.
        /// </summary>
        private readonly VersionIncrementationOptions _incremenationOptions;
        private ISemanticVersion? _currentVersion;
        private bool? _doesCurrentVersionContainsMajorIncrement;
        private bool? _doesCurrentVersionContainsMinorIncrement;
        private bool? _doesCurrentVersionContainsPatchIncrement;
        private VersionHeightInformations? _doesCurrentVersionContainsHeightIncrement;
        private bool? _canFlowDownstreamMajor;
        private bool? _canFlowDownstreamMinor;
        private bool? _isPreReleaseOfCurrentVersionAdapted;
        private bool? _isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="versionCalculationOptions"></param>
        public VersionIncrementContext(VersionIncrementationOptions versionCalculationOptions)
        {
            _incremenationOptions = versionCalculationOptions;

            IsStartVersionPreReleaseAlternating = _incremenationOptions.IsPostTransformerUsable
                ? _incremenationOptions.PostTransformer.IsTransformationResultingIntoPreRelease != StartVersion.IsPreRelease
                    // Or, the start version and prospective post transfromer indicates both a pre-release and ..
                    || (_incremenationOptions.PostTransformer.IsTransformationResultingIntoPreRelease && StartVersion.IsPreRelease
                        // .. the start version pre-release does not start with prospective pre-release
                        && !_incremenationOptions.PostTransformer.StartsWithProspectivePreRelease(StartVersion.PreReleaseIdentifiers))
                : false;

            _isPreReleaseOfCurrentVersionAdapted = IsStartVersionPreReleaseAlternating
                // First current version can never be adapted
                ? false
                // Current version is not adaptable
                : null;

            _isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted = _isPreReleaseOfCurrentVersionAdapted;
            HeightIdentifierTransformer = versionCalculationOptions.CreateHeightIdentifierTransformer();
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="context"></param>
        public VersionIncrementContext(VersionIncrementContext context)
        {
            _incremenationOptions = context._incremenationOptions;

            IsStartVersionPreReleaseAlternating = context.IsStartVersionPreReleaseAlternating;
            _isPreReleaseOfCurrentVersionAdapted = context._isPreReleaseOfCurrentVersionAdapted;
            _isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted = context._isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted;

            HeightIdentifierTransformer = context.HeightIdentifierTransformer;
            _currentVersion = context.CurrentVersion;
            _doesCurrentVersionContainsMajorIncrement = context._doesCurrentVersionContainsMajorIncrement;
            _doesCurrentVersionContainsMinorIncrement = context._doesCurrentVersionContainsMinorIncrement;
            _doesCurrentVersionContainsPatchIncrement = context._doesCurrentVersionContainsPatchIncrement;
            _doesCurrentVersionContainsHeightIncrement = context._doesCurrentVersionContainsHeightIncrement;
        }

        private bool CurrentVersionContainsMajorIncrement() =>
            (IsCurrentVersionEquivalentToStartVersionAfterPreReleaseAdaption == true
                && IsRightSideOfMajorOfCurrentVersionCoreZeroed)
            || CurrentVersion.Major > _incremenationOptions.StartVersion.Major;

        private bool CurrentVersionContainsMinorIncrement() =>
            (IsCurrentVersionEquivalentToStartVersionAfterPreReleaseAdaption == true
                && IsRightSideOfMinorOfCurrentVersionCoreZeroed)
            || DoesCurrentVersionContainsMajorIncrement
            || CurrentVersion.Major == _incremenationOptions.StartVersion.Major
                && CurrentVersion.Minor > _incremenationOptions.StartVersion.Minor;

        private bool CurrentVersionContainsPatchIncrement() =>
            IsCurrentVersionEquivalentToStartVersionAfterPreReleaseAdaption == true
            || DoesCurrentVersionContainsMajorIncrement
            || DoesCurrentVersionContainsMinorIncrement
            || CurrentVersion.Major == _incremenationOptions.StartVersion.Major
                && CurrentVersion.Minor == _incremenationOptions.StartVersion.Minor
                && CurrentVersion.Patch > _incremenationOptions.StartVersion.Patch;

        private VersionHeightInformations CurrentVersionContainsHeightIncrement()
        {
            var startVersionTransformResult = HeightIdentifierTransformer.Transform(_incremenationOptions.StartVersion);
            var currentVersionTransformResult = HeightIdentifierTransformer.Transform(CurrentVersion);
            var versionNumberParser = CurrentVersion.GetParserOrStrict().VersionParser;

            _ = currentVersionTransformResult.TryParseHeight(versionNumberParser, out var currentVersionHeight);
            _ = startVersionTransformResult.TryParseHeight(versionNumberParser, out var startVersionHeight);

            var currentVersionContainsHeightIncrement = currentVersionHeight.HasValue && !startVersionHeight.HasValue
                || currentVersionHeight > startVersionHeight;

            return new VersionHeightInformations(currentVersionTransformResult, currentVersionHeight, currentVersionContainsHeightIncrement);
        }

        private bool? GetOrCalculateWhetherPreReleaseOfCurrentVersionIsAdapted()
        {
            var isPreReleaseOfCurrentVersionAlreadyAdapted = _isPreReleaseOfCurrentVersionAdapted;

            if (isPreReleaseOfCurrentVersionAlreadyAdapted.GetValueOrDefault(true)) {
                // Null or true
                return isPreReleaseOfCurrentVersionAlreadyAdapted;
            }

            bool isCurrentVersionPreReleaseAdapted;

            if (PostVersionPreReleaseTransformer!.StartsWithProspectivePreRelease(CurrentVersion.PreReleaseIdentifiers)) {
                isCurrentVersionPreReleaseAdapted = true;
            } else {
                isCurrentVersionPreReleaseAdapted = false;
            }

            if (isPreReleaseOfCurrentVersionAlreadyAdapted != isCurrentVersionPreReleaseAdapted) {
                _isPreReleaseOfCurrentVersionAdapted = isCurrentVersionPreReleaseAdapted;
            }

            return isCurrentVersionPreReleaseAdapted;
        }

        private bool? GetOrCalculateWhetherPreReleaseOfStartVersionCoreEquivalentCurrentVersionIsAdapted()
        {
            var isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAlreadyAdapted = _isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted;

            if (isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAlreadyAdapted.GetValueOrDefault(true)) {
                // Null or true
                return isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAlreadyAdapted;
            }

            bool? isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted;

            if (!SemanticVersionComparer.Version.Equals(CurrentVersion, StartVersion)) {
                isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted = null;
            } else {
                isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted = IsPreReleaseOfCurrentVersionAdapted;
            }

            if (isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAlreadyAdapted != isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted) {
                _isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted = isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted;
            }

            return isPreReleaseOfStartVersionCoreEquivalentCurrentVersionAdapted;
        }

        private void ResetCurrentVersionDerivedInformations()
        {
            _doesCurrentVersionContainsMajorIncrement = null;
            _doesCurrentVersionContainsMinorIncrement = null;
            _doesCurrentVersionContainsPatchIncrement = null;
            _doesCurrentVersionContainsHeightIncrement = null;
        }

        /// <summary>
        /// Provides height informations.
        /// </summary>
        public sealed class VersionHeightInformations
        {
            /// <summary>
            /// The transform result from using the height convention.
            /// </summary>
            public HeightConventionTransformResult TransformResult { get; }
            /// <summary>
            /// The parsed height number.
            /// </summary>
            public uint? HeightNumber { get; }

            private readonly bool _isIncremented;

            /// <summary>
            /// Creates an instance of this type.
            /// </summary>
            /// <param name="conventionTransformResult"></param>
            /// <param name="heightNumber"></param>
            /// <param name="isIncremented"></param>
            internal VersionHeightInformations(HeightConventionTransformResult conventionTransformResult, uint? heightNumber, bool isIncremented)
            {
                TransformResult = conventionTransformResult;
                HeightNumber = heightNumber;
                _isIncremented = isIncremented;
            }

            /// <summary>
            /// True if height has been incremented at least once.
            /// </summary>
            /// <param name="informations"></param>
            public static implicit operator bool(VersionHeightInformations informations) =>
                informations._isIncremented;
        }
    }
}
