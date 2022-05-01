using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Console;

/// <summary>
/// The program class.
/// </summary>
public static class ConsoleProgram
{
    /// <summary>
    /// Runs console application.
    /// </summary>
    /// <param name="args">arguments</param>
    /// <param name="pluginDescriptors"></param>
    /// <returns>exit code</returns>
    public static async Task<int> RunAsync(string[] args, IEnumerable<PluginDescriptor>? pluginDescriptors = null)
    {
        using var pluginRegistry = new PluginRegistry();

        await pluginRegistry.RegisterAsync<IVersioningPresetsPlugin, VersioningPresetsPlugin>();
        await pluginRegistry.RegisterAsync<ICommandLinePlugin, CommandLinePlugin>();
        await pluginRegistry.RegisterAsync<ILoggingPlugin, LoggingPlugin>();
        await pluginRegistry.RegisterAsync<IConfigurationPlugin, ConfigurationPlugin>();
        await pluginRegistry.RegisterAsync<IGitPlugin, GitPlugin>();
        await pluginRegistry.RegisterAsync<INextVersionPlugin, NextVersionPlugin>();
        await pluginRegistry.RegisterAsync<VersionCalculationPerfomancePlugin>();

        if (pluginDescriptors is not null) {
            foreach (var pluginDescriptor in pluginDescriptors) {
                await pluginRegistry.RegisterAsync(pluginDescriptor.PluginType, pluginDescriptor.Plugin);
            }
        }

        var pluginEventAggregator = new PluginEventAggregator();

        var pluginExecutor = new PluginExecutor(pluginRegistry, pluginEventAggregator);
        await pluginExecutor.ExecuteAsync();

        int exitCode = (int)ExitCode.NotExecuted;
        using var exitCodeSubscription = pluginEventAggregator.GetEvent<CommandLineEvents.InvokedRootCommand>().Subscribe(i => exitCode = i);

        pluginEventAggregator.PublishEvent(CommandLineEvents.SetCommandLineArgs.Discriminator, args);
        pluginEventAggregator.PublishEvent<CommandLineEvents.ParseCommandLineArgs>();

        if (exitCode == (int)ExitCode.NotExecuted) {
            pluginEventAggregator.PublishEvent<LoggingEvents.EnableLoggingInfrastructure>();
            pluginEventAggregator.PublishEvent<ConfigurationEvents.CreateConfiguration>();
            pluginEventAggregator.PublishEvent<CommandLineEvents.InvokeRootCommand>();
        }

        await pluginExecutor.DestroyAsync();

        if (exitCode == (int)ExitCode.NotExecuted) {
            throw new InvalidOperationException("The command line plugin was not running");
        }

        return exitCode;
    }

    /// <summary>
    /// Entry point of console application.
    /// </summary>
    /// <param name="args"></param>
    /// <returns>exit code</returns>
    public static Task<int> Main(string[] args) => RunAsync(args);
}
