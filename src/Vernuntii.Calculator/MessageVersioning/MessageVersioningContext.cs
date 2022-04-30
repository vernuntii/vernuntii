using Vernuntii.HeightConventions.Transformation;
using Vernuntii.SemVer;

namespace Vernuntii.MessageVersioning
{
    /// <summary>
    /// Represents the context for <see cref="VersionIncrementBuilder"/>.
    /// </summary>
    public record MessageVersioningContext
    {
        /// <summary>
        /// The options of on-going calculation.
        /// </summary>
        public SemanticVersionCalculationOptions VersionCalculationOptions { get; }

        /// <summary>
        /// The current version before the next transformation is applied.
        /// </summary>
        public ISemanticVersion CurrentVersion {
            get => _currentVersion ?? VersionCalculationOptions.StartVersion;

            init {
                _currentVersion = value;
                ResetCurrentVersionContainingInformations();
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
        /// The height identifier transformer.
        /// </summary>
        public HeightConventionTransformer HeightIdentifierTransformer { get; }

        private ISemanticVersion? _currentVersion;
        private bool? _doesCurrentVersionContainsMajorIncrement;
        private bool? _doesCurrentVersionContainsMinorIncrement;
        private bool? _doesCurrentVersionContainsPatchIncrement;
        private VersionHeightInformations? _doesCurrentVersionContainsHeightIncrement;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="versionCalculationOptions"></param>
        public MessageVersioningContext(SemanticVersionCalculationOptions versionCalculationOptions)
        {
            VersionCalculationOptions = versionCalculationOptions;
            HeightIdentifierTransformer = versionCalculationOptions.CreateHeightIdentifierTransformer();
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="context"></param>
        public MessageVersioningContext(MessageVersioningContext context)
        {
            VersionCalculationOptions = context.VersionCalculationOptions;
            HeightIdentifierTransformer = context.HeightIdentifierTransformer;
            _currentVersion = context.CurrentVersion;
            _doesCurrentVersionContainsMajorIncrement = context._doesCurrentVersionContainsMajorIncrement;
            _doesCurrentVersionContainsMinorIncrement = context._doesCurrentVersionContainsMinorIncrement;
            _doesCurrentVersionContainsPatchIncrement = context._doesCurrentVersionContainsPatchIncrement;
            _doesCurrentVersionContainsHeightIncrement = context._doesCurrentVersionContainsHeightIncrement;
        }

        private bool CurrentVersionContainsMajorIncrement() =>
            CurrentVersion.Major > VersionCalculationOptions.StartVersion.Major;

        private bool CurrentVersionContainsMinorIncrement() =>
            DoesCurrentVersionContainsMajorIncrement
            || (CurrentVersion.Major == VersionCalculationOptions.StartVersion.Major
                && CurrentVersion.Minor > VersionCalculationOptions.StartVersion.Minor);

        private bool CurrentVersionContainsPatchIncrement() =>
            DoesCurrentVersionContainsMajorIncrement
            || DoesCurrentVersionContainsMinorIncrement
            || (CurrentVersion.Major == VersionCalculationOptions.StartVersion.Major
                && CurrentVersion.Minor == VersionCalculationOptions.StartVersion.Minor
                && CurrentVersion.Patch > VersionCalculationOptions.StartVersion.Patch);

        private VersionHeightInformations CurrentVersionContainsHeightIncrement()
        {
            var startVersionTransformResult = HeightIdentifierTransformer.Transform(VersionCalculationOptions.StartVersion);
            var currentVersionTransformrResult = HeightIdentifierTransformer.Transform(CurrentVersion);
            var versionNumberParser = CurrentVersion.GetParserOrStrict().VersionNumberParser;

            _ = currentVersionTransformrResult.TryParseHeight(versionNumberParser, out var currentVersionHeight);
            _ = startVersionTransformResult.TryParseHeight(versionNumberParser, out var startVersionHeight);

            var currentVersionContainsHeightIncrement = (currentVersionHeight.HasValue && !startVersionHeight.HasValue)
                || currentVersionHeight > startVersionHeight;

            return new VersionHeightInformations(currentVersionTransformrResult, currentVersionHeight, currentVersionContainsHeightIncrement);
        }

        private void ResetCurrentVersionContainingInformations()
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

            private bool _isIncremented;

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
