using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Default implementation for incrementing the patch version by one.
    /// </summary>
    public sealed class NextPatchVersionTransformer : IVersionTransformer
    {
        /// <summary>
        /// Default instance.
        /// </summary>
        public static readonly NextPatchVersionTransformer Default = new();

        bool IVersionTransformer.DoesNotTransform => false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="version"></param>
        public ISemanticVersion TransformVersion(ISemanticVersion version) =>
            version.With().Patch(version.Patch + 1).ToVersion();
    }
}
