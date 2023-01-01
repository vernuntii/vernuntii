namespace Vernuntii.VersionPersistence
{
    /// <summary>
    /// The options for <see cref="VersionCacheDirectory"/>
    /// </summary>
    public class VersionCacheDirectoryOptions
    {
        /// <summary>
        /// The base directory where the cache folder is located.
        /// </summary>
        public string BaseDirectory { get; }

        /// <summary>
        /// The cache folder name. Default is "vernuntii_cache".
        /// </summary>
        public string CacheFolderName { get; } = "vernuntii_cache";

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="baseDirectory">The base directory where the cache folder is located.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public VersionCacheDirectoryOptions(string baseDirectory) =>
            BaseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
    }
}
