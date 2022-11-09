using System.Diagnostics.CodeAnalysis;
using Vernuntii.PluginSystem.Lifecycle;
using Vernuntii.VersionCaching;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Represents the up-to date mechanism to check whether you need to re-calculate the next version.
    /// </summary>
    public interface IVersionCacheCheckPlugin : IPlugin
    {
        /// <summary>
        /// The up-to-date version cache from fast check.
        /// </summary>
        IVersionCache? VersionCache { get; }

        /// <summary>
        /// <see langword="true"/> means the next version is up to date.
        /// </summary>
        [MemberNotNullWhen(true, nameof(VersionCache))]
        bool IsCacheUpToDate { get; }
    }
}
