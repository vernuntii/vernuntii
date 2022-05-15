using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Default implementation for incrementing the minor version by one
    /// and resetting patch version to zero.
    /// </summary>
    public sealed class NextMinorVersionTransformer : IVersionTransformer
    {
        /// <summary>
        /// Default instance.
        /// </summary>
        public readonly static NextMinorVersionTransformer Default = new NextMinorVersionTransformer();

        bool IVersionTransformer.DoesNotTransform => false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="version"></param>
        public ISemanticVersion TransformVersion(ISemanticVersion version) => version.With()
            .Minor(version.Minor + 1)
            .Patch(0)
            .ToVersion();
    }
}
