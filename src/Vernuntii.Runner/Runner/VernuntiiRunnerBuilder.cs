using System.Diagnostics;
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
        private static void AddRequiredPlugins(IPluginRegistrar builder)
        {
            builder.Add<ILoggingPlugin, LoggingPlugin>();
            builder.Add<IServicesPlugin, ServicesPlugin>();
            builder.Add<ICommandLinePlugin, CommandLinePlugin>();
            builder.Add<IConfigurationPlugin, ConfigurationPlugin>();
        }

        private static void AddRequiredPluginServices(IServiceCollection services)
        {
            services.TryAddSingleton<ILoggerFactory>(sp => sp.GetRequiredService<ILoggingPlugin>());
            services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
        }

        private readonly List<Action<IPluginRegistrar>> _configurePluginProviderBuilderActions = new() {
            AddRequiredPlugins
        };

        private readonly PluginRegistryBuilder _pluginRegistryBuilder;
        private readonly VernuntiiRunnerBuilderOptions _options;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public VernuntiiRunnerBuilder(VernuntiiRunnerBuilderOptions options)
        {
            _pluginRegistryBuilder = new();
            ConfigurePluginServices(AddRequiredPluginServices);

            if (options.AddNextVersionRequirements) {
                ConfigurePlugins(plugins => plugins.AddNextVersionRequirements());
            }
            _options = options;
        }

        /// <summary>
        /// Creates an instance of this type using <see cref="VernuntiiRunnerBuilderOptions.Default"/>.
        /// </summary>
        public VernuntiiRunnerBuilder()
            : this(VernuntiiRunnerBuilderOptions.Default)
        {
        }

        //private readonly List<Action<IServiceCollection>> _configurePluginServicesActions = new() {
        //    AddRequiredPluginServices
        //};

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
            _pluginRegistryBuilder.ConfigurePluginServices(configure);
            return this;
        }

        /// <inheritdoc/>
        public IVernuntiiRunner Build(string[]? args)
        {
            if (_options.CheckDaemonizeFlag && VernuntiiDaemonizer.ShouldDaemonize(args, out var trimmedConsoleArguments)) {
                return new VernuntiiDaemonizer(trimmedConsoleArguments);
            }

            PluginRegistrar CreatePluginRegistrar()
            {
                var pluginProviderBuilder = new PluginRegistrar();

                foreach (var configurePlugin in _configurePluginProviderBuilderActions) {
                    configurePlugin(pluginProviderBuilder);
                }

                return pluginProviderBuilder;
            }

            var pluginRegistrar = CreatePluginRegistrar();

            return new VernuntiiRunner(_pluginRegistryBuilder, pluginRegistrar) {
                ConsoleArguments = args ?? Array.Empty<string>()
            };
        }
    }
}
