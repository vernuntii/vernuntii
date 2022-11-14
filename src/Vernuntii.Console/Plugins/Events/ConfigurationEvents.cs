using Microsoft.Extensions.Configuration;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="IConfigurationPlugin"/>.
    /// </summary>
    public sealed class ConfigurationEvents
    {
        /// <summary>
        /// Event is happening when the configuration builder has been configured.
        /// </summary>
        public readonly static SubjectEvent ConfiguredConfigurationBuilder = new SubjectEvent();

        /// <summary>
        /// Event is happening when the configuration is getting created.
        /// </summary>
        public readonly static SubjectEvent CreateConfiguration = new SubjectEvent();

        /// <summary>
        /// Event is happening when the configuration got created.
        /// </summary>
        public readonly static SubjectEvent<IConfiguration> CreatedConfiguration = new SubjectEvent<IConfiguration>();
    }
}
