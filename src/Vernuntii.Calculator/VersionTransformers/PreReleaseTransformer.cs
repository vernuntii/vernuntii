using Vernuntii.SemVer;

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
        public SemanticVersion TransformVersion(SemanticVersion version)
        {
            if (string.IsNullOrEmpty(ProspectivePreRelease)) {
                return version.With.PreRelease(default(string));
            }

            var currentIdentifiers = version.PreReleaseIdentifiers.ToArray();
            var prospectiveIdentifiers = ProspectivePreRelease.Split('.').ToArray();

            if (prospectiveIdentifiers.Length >= currentIdentifiers.Length) {
                return version.With.PreRelease(prospectiveIdentifiers);
            }

            for (var i = 0; i < prospectiveIdentifiers.Length; i++) {
                currentIdentifiers[i] = prospectiveIdentifiers[i];
            }

            return version.With.PreRelease(currentIdentifiers);
        }
    }
}
