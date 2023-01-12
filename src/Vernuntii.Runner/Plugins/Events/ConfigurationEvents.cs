using Microsoft.Extensions.Configuration;
using Vernuntii.PluginSystem.Reactive;

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
        public static readonly EventDiscriminator<ConfiguredConfigurationBuilderResult> OnConfiguredConfigurationBuilder = EventDiscriminator.New<ConfiguredConfigurationBuilderResult>();

        /// <summary>
        /// Event is happening when the configuration is getting created.
        /// </summary>
        public static readonly EventDiscriminator CreateConfiguration = EventDiscriminator.New();

        /// <summary>
        /// Event is happening when the configuration got created.
        /// </summary>
        public static readonly EventDiscriminator<IConfiguration> OnCreatedConfiguration = EventDiscriminator.New<IConfiguration>();

        /// <summary>
        /// The result of <see cref="OnConfiguredConfigurationBuilder"/>.
        /// </summary>
        public sealed class ConfiguredConfigurationBuilderResult
        {
            internal ConfiguredConfigurationBuilderResult()
            {
            }

            /// <summary>
            /// The config path.
            /// </summary>
            public string? ConfigPath { get; init; }
        }
    }
}
