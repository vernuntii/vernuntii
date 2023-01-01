using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer;

namespace Vernuntii.VersionPersistence
{
    /// <summary>
    /// The attributes regarding the version foundation.
    /// </summary>
    public interface IVersionCache : IImmutableVersionCacheDataTuples
    {
        /// <summary>
        /// The version.
        /// </summary>
        ISemanticVersion Version { get; }

        /// <summary>
        /// Expiration time.
        /// </summary>
        public ExpirationTime ExpirationTime { get; }

        /// <summary>
        /// Last access time.
        /// </summary>
        public DateTime? LastAccessTime { get; }

        /// <summary>
        /// Tries to get data by specifying <paramref name="part"/>, but in-built cache parts are skiped.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        bool TryGetAdditionalData(IVersionCachePart part, out object? data, [NotNullWhen(true)] out Type? dataType);
    }
}

