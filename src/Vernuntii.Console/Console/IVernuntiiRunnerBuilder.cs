using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Plugins;
using Vernuntii.PluginSystem;

namespace Vernuntii.Console
{
    /// <summary>
    /// Represents the builder to create the main entry point of <see cref="Vernuntii"/>.
    /// </summary>
    public interface IVernuntiiRunnerBuilder
    {
        /// <summary>
        /// Configures the plugins.
        /// </summary>
        /// <param name="configurePlugins"></param>
        IVernuntiiRunnerBuilder ConfigurePlugins(Action<IPluginProviderBuilder> configurePlugins);

        /// <summary>
        /// Configures the dependencies of plugins. This does not add plugins, only their dependencies.
        /// </summary>
        /// <param name="configure"></param>
        IVernuntiiRunnerBuilder ConfigurePluginServices(Action<IServiceCollection> configure);

        /// <summary>
        /// Builds the runner for <see cref="Vernuntii"/>.
        /// </summary>
        /// <param name="args"></param>
        VernuntiiRunner Build(string[]? args);
    }
}
