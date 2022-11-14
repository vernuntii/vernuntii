using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Default implementation for not incrementing the version.
    /// </summary>
    public sealed class NoneVersionTransformer : IVersionTransformer
    {
        /// <summary>
        /// Default instance.
        /// </summary>
        public static readonly NoneVersionTransformer Default = new();

        bool IVersionTransformer.DoesNotTransform => true;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="version"></param>
        public ISemanticVersion TransformVersion(ISemanticVersion version) =>
            version;
    }
}
