namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Transforms version with alternative pre-release.
    /// </summary>
    public interface IPreReleaseTransformer : IVersionTransformer
    {
        /// <summary>
        /// The pre-release to be used to transform version.
        /// </summary>
        string? ProspectivePreRelease { get; }
    }
}
