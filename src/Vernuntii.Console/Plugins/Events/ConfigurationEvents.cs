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
        public static readonly EventDiscriminator<ConfiguredConfigurationBuilderResult> ConfiguredConfigurationBuilder = new();

        /// <summary>
        /// Event is happening when the configuration is getting created.
        /// </summary>
        public static readonly EventDiscriminator CreateConfiguration = new();

        /// <summary>
        /// Event is happening when the configuration got created.
        /// </summary>
        public static readonly EventDiscriminator<IConfiguration> CreatedConfiguration = new();

        /// <summary>
        /// The result of <see cref="ConfiguredConfigurationBuilder"/>.
        /// </summary>
        public sealed class ConfiguredConfigurationBuilderResult
        {
            internal ConfiguredConfigurationBuilderResult()
            {
            }

            public string? ConfigPath { get; init; }
        }
    }
}
