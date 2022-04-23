using Microsoft.Extensions.Configuration;

namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Events for <see cref="IConfigurationPlugin"/>.
    /// </summary>
    public sealed class ConfigurationEvents
    {
        /// <summary>
        /// Event is happening when the configuration is getting created.
        /// </summary>
        public sealed class CreateConfiguration : PubSubEvent { 
        }

        /// <summary>
        /// Event is happening when the configuration got created.
        /// </summary>
        public sealed class CreatedConfiguration : PubSubEvent<CreatedConfiguration, IConfiguration>
        {
        }
    }
}
