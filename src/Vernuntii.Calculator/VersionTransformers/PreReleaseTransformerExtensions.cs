using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers;

internal static class PreReleaseTransformerExtensions
{
    internal static bool StartsWithProspectivePreRelease(this IPreReleaseTransformer transformer, IEnumerable<string> identifiers) =>
        PreReleaseIdentifierComparer.Default.Equals(transformer.ProspectivePreRelease, identifiers.Take(transformer.ProspectivePreReleaseDots + 1));
}
