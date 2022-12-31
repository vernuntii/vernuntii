using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Vernuntii.Plugins;
using Vernuntii.PluginSystem;

namespace Vernuntii.Runner
{
    /// <summary>
    /// Builds the runner for <see cref="Vernuntii"/>.
    /// </summary>
    public sealed class VernuntiiRunnerBuilder : IVernuntiiRunnerBuilder
    {
        /// <summary>
        /// Creates a builder for a Vernuntii runner including capabilities to calculate the next version.
        /// </summary>
        public static IVernuntiiRunnerBuilder ForNextVersion() => new VernuntiiRunnerBuilder()
            .ConfigurePlugins(builder => builder.AddNextVersion());

        private static void AddRequiredPlugins(IPluginRegistrar builder)
        {
            builder.Add<ILoggingPlugin, LoggingPlugin>();
            builder.Add<IServicesPlugin, ServicesPlugin>();
            builder.Add<ICommandLinePlugin, CommandLinePlugin>();
            builder.Add<IConfigurationPlugin, ConfigurationPlugin>();
        }

        private static void AddPluginServices(IServiceCollection services)
        {
            services.TryAddSingleton<ILoggerFactory>(sp => sp.GetRequiredService<ILoggingPlugin>());
            services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
        }

        private readonly List<Action<IPluginRegistrar>> _configurePluginProviderBuilderActions = new() {
            AddRequiredPlugins
        };

        private readonly List<Action<IServiceCollection>> _configurePluginServicesActions = new() {
            AddPluginServices
        };

        /// <inheritdoc/>
        public IVernuntiiRunnerBuilder ConfigurePlugins(Action<IPluginRegistrar> configure)
        {
            if (configure is null) {
                throw new ArgumentNullException(nameof(configure));
            }

            _configurePluginProviderBuilderActions.Add(configure);
            return this;
        }

        /// <inheritdoc/>
        public IVernuntiiRunnerBuilder ConfigurePluginServices(Action<IServiceCollection> configure)
        {
            if (configure is null) {
                throw new ArgumentNullException(nameof(configure));
            }

            _configurePluginServicesActions.Add(configure);
            return this;
        }

        /// <inheritdoc/>
        public VernuntiiRunner Build(string[]? args)
        {
            var pluginProviderBuilder = CreatePluginProviderBuilder();
            var pluginRegistry = CreatePluginRegistry();

            return new VernuntiiRunner(pluginRegistry) {
                ConsoleArguments = args ?? Array.Empty<string>()
            };

            PluginRegistrar CreatePluginProviderBuilder()
            {
                var pluginProviderBuilder = new PluginRegistrar();

                foreach (var configurePlugin in _configurePluginProviderBuilderActions) {
                    configurePlugin(pluginProviderBuilder);
                }

                return pluginProviderBuilder;
            }

            PluginRegistry CreatePluginRegistry()
            {
                var pluginRegistryFactory = new PluginRegistryFactory(pluginProviderBuilder);

                return pluginRegistryFactory.Create(services => {
                    foreach (var configureServices in _configurePluginServicesActions) {
                        configureServices(services);
                    }
                });
            }
        }
    }
}
