namespace Vernuntii.VersionFoundation.Caching
{
    /// <summary>
    /// The options for <see cref="SemanticVersionFoundationCache"/>.
    /// </summary>
    public class SemanticVersionFoundationCacheOptions
    {
        /// <summary>
        /// The cache folder name. Default is "vernuntii_cache".
        /// </summary>
        public string CacheFolderName { get; } = "vernuntii_cache";

        /// <summary>
        /// The time to wait until lock attempt is aborted. Default are 10 seconds.
        /// </summary>
        public int LockAttemptSeconds { get; } = 10 * 1000;
    }
}
