using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Default implementation for incrementing the major version by one
    /// and resetting minor and patch version to zero.
    /// </summary>
    public sealed class NextMajorVersionTransformer : ISemanticVersionTransformer
    {
        /// <summary>
        /// Default instance.
        /// </summary>
        public readonly static NextMajorVersionTransformer Default = new NextMajorVersionTransformer();

        bool ISemanticVersionTransformer.DoesNotTransform => false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="version"></param>
        public ISemanticVersion TransformVersion(ISemanticVersion version) => 
            version.With().Version(version.Major + 1, 0, 0).ToVersion();
    }
}
