using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Vernuntii.Plugins;
using Vernuntii.PluginSystem;

namespace Vernuntii.Console
{
    /// <summary>
    /// Builds the runner for <see cref="Vernuntii"/>.
    /// </summary>
    public sealed class VernuntiiRunnerBuilder : IVernuntiiRunnerBuilder
    {
        private static void AddDefaultPlugins(IPluginProviderBuilder builder)
        {
            builder.Add<ILoggingPlugin, LoggingPlugin>();
            builder.Add<IGlobalServicesPlugin, GlobalServicesPlugin>();
            builder.Add<IVersioningPresetsPlugin, VersioningPresetsPlugin>();
            builder.Add<ICommandLinePlugin, CommandLinePlugin>();
            builder.Add<IConfigurationPlugin, ConfigurationPlugin>();
            builder.Add<IGitPlugin, GitPlugin>();
            builder.Add<IVersionCacheCheckPlugin, VersionCacheCheckPlugin>();
            builder.Add<INextVersionPlugin, NextVersionPlugin>();
        }

        private static void AddPluginServices(IServiceCollection services)
        {
            services.TryAddSingleton<ILoggerFactory>(sp => sp.GetRequiredService<ILoggingPlugin>());
            services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
        }

        private List<Action<IPluginProviderBuilder>> _configurePluginProviderBuilderActions = new() {
            AddDefaultPlugins
        };

        private List<Action<IServiceCollection>> _configurePluginServicesActions = new() {
            AddPluginServices
        };

        /// <inheritdoc/>
        public IVernuntiiRunnerBuilder ConfigurePlugins(Action<IPluginProviderBuilder> configure)
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
                ConsoleArgs = args ?? Array.Empty<string>()
            };

            PluginProviderBuilder CreatePluginProviderBuilder()
            {
                var pluginProviderBuilder = new PluginProviderBuilder();

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
