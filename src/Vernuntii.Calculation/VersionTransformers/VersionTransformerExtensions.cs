using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Extension methods for 
    /// </summary>
    public static class VersionTransformerExtensions
    {
        /// <summary>
        /// Checks <paramref name="transformer"/> is not null and can transform.
        /// </summary>
        /// <param name="transformer"></param>
        public static bool CanTransform([NotNullWhen(true)] this IVersionTransformer? transformer) =>
            transformer is not null && !transformer.DoesNotTransform;

        /// <summary>
        /// Transforms the version.
        /// </summary>
        /// <param name="versionTransformer"></param>
        /// <param name="version"></param>
        public static ISemanticVersion TransformVersion(this IVersionTransformer versionTransformer, SemanticVersion version) =>
            versionTransformer.TransformVersion(version);

        /// <summary>
        /// Transforms the version.
        /// </summary>
        /// <param name="versionTransformers"></param>
        /// <param name="startVersion"></param>
        public static ISemanticVersion TransformVersion(this IEnumerable<IVersionTransformer> versionTransformers, ISemanticVersion startVersion)
        {
            var preflightVersion = startVersion;

            foreach (var versionTransformer in versionTransformers) {
                if (versionTransformer is null || versionTransformer.DoesNotTransform) {
                    continue;
                }

                preflightVersion = versionTransformer.TransformVersion(preflightVersion);
            }

            return preflightVersion;
        }
    }
}
