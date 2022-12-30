using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Default implementation for incrementing the major version by one
    /// and resetting minor and patch version to zero.
    /// </summary>
    public sealed class NextMajorVersionTransformer : IVersionTransformer
    {
        /// <summary>
        /// Default instance.
        /// </summary>
        public static readonly NextMajorVersionTransformer Default = new();

        bool IVersionTransformer.DoesNotTransform => false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="version"></param>
        public ISemanticVersion TransformVersion(ISemanticVersion version) =>
            version.With().Version(version.Major + 1, 0, 0).ToVersion();
    }
}
