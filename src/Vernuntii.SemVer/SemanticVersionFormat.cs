namespace Vernuntii.SemVer
{
    /// <summary>
    /// The available formats to present the version.
    /// </summary>
    public enum SemanticVersionFormat
    {
        /// <summary>
        /// The prefix.
        /// </summary>
        Prefix = 1,
        /// <summary>
        /// The version core.
        /// </summary>
        Version = 2,
        /// <summary>
        /// The pre-release.
        /// </summary>
        PreRelease = 4,
        /// <summary>
        /// The build.
        /// </summary>
        Build = 8,
        /// <summary>
        /// The version core and pre-release.
        /// </summary>
        VersionRelease = Version | PreRelease,
        /// <summary>
        /// The version core, pre-release and build.
        /// </summary>
        VersionReleaseBuild = VersionRelease | Build,
        /// <summary>
        /// The full version representation.
        /// </summary>
        SemanticVersion = Prefix | Version | PreRelease | Build,
    }
}
