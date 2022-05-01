using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Events for <see cref="IVersioningPresetsPlugin"/>.
    /// </summary>
    public static class VersioningPresetsEvents
    {
        /// <summary>
        /// Event when globales services has been configured.
        /// </summary>
        public readonly static SubjectEvent<IServiceCollection> ConfiguredGlobalServices = new SubjectEvent<IServiceCollection>();
    }
}
