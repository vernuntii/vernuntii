using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Default implementation for not incrementing the version.
    /// </summary>
    public sealed class NoneVersionTransformer : ISemanticVersionTransformer
    {
        /// <summary>
        /// Default instance.
        /// </summary>
        public readonly static NoneVersionTransformer Default = new NoneVersionTransformer();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="version"></param>
        public SemanticVersion TransformVersion(SemanticVersion version) =>
            version;

        bool ISemanticVersionTransformer.DoesNotTransform => true;
    }
}
