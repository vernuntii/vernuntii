using Vernuntii.VersionPersistence;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Allows you to access the output of the next version.
    /// </summary>
    public sealed class NextVersionResult
    {
        /// <summary>
        /// The formatted string representation of <see cref="VersionCache"/>,
        /// that you could format by the various "--presentation-"-options.
        /// </summary>
        public string VersionCacheString { get; }

        /// <summary>
        /// The version cache, that hold besides the version, version-related data.
        /// </summary>
        public IVersionCache VersionCache { get; }

        internal NextVersionResult(string versionCacheString, IVersionCache versionCache)
        {
            VersionCacheString = versionCacheString ?? throw new ArgumentNullException(nameof(versionCacheString));
            VersionCache = versionCache ?? throw new ArgumentNullException(nameof(versionCache));
        }
    }
}
