using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Extension methods for 
    /// </summary>
    public static class SemanticVersionTransformerExtensions
    {
        /// <summary>
        /// Transform the version.
        /// </summary>
        /// <param name="versionTransformer"></param>
        /// <param name="version"></param>
        public static ISemanticVersion TransformVersion(this ISemanticVersionTransformer versionTransformer, SemanticVersion version) =>
            versionTransformer.TransformVersion(version);
    }
}
