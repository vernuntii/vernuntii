using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// The cache directory.
    /// </summary>
    public interface IVersionCacheDirectory
    {
        /// <summary>
        /// The cache directory.
        /// </summary>
        string? CacheDirectoryPath { get; }

        /// <summary>
        /// Ensures existing cache directory exists.
        /// </summary>
        [MemberNotNull(nameof(CacheDirectoryPath))]
        void CreateCacheDirectoryIfNotExisting();
    }
}
