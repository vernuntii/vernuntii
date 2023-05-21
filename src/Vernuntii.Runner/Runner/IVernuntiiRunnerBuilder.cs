using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem;

namespace Vernuntii.Runner
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
        IVernuntiiRunnerBuilder ConfigurePlugins(Action<IPluginRegistrar> configurePlugins);

        /// <summary>
        /// Configures the dependencies of plugins. This does not add plugins, only their dependencies.
        /// </summary>
        /// <param name="configureServices"></param>
        IVernuntiiRunnerBuilder ConfigurePluginServices(Action<IServiceCollection> configureServices);

        /// <summary>
        /// Builds the <see cref="Vernuntii"/> runner.
        /// </summary>
        /// <param name="args"></param>
        IVernuntiiRunner Build(string[]? args);
    }
}
