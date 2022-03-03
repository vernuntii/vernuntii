namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Sets pre-release of version.
    /// </summary>
    public class PreReleaseTransformer : IPreReleaseTransformer
    {
        /// <inheritdoc/>
        public string? ProspectivePreRelease { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="preRelease"></param>
        public PreReleaseTransformer(string? preRelease) =>
            ProspectivePreRelease = preRelease;

        /// <inheritdoc/>
        public SemanticVersion TransformVersion(SemanticVersion version) =>
            version.With.PreRelease(ProspectivePreRelease);
    }
}
