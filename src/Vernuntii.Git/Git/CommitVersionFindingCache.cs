namespace Vernuntii.Git
{
    /// <summary>
    /// Cache for <see cref="CommitVersionFindingCache"/>.
    /// </summary>
    public record CommitVersionFindingCache()
    {
        /// <summary>
        /// The found commit version.
        /// </summary>
        public CommitVersion? CommitVersion { get; init; }
    }
}
