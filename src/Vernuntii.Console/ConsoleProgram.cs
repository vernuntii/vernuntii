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
        using var plugins = new PluginRegistry();

        await plugins.RegisterAsync<IVersioningPresetsPlugin, VersioningPresetsPlugin>();
        await plugins.RegisterAsync<ICommandLinePlugin, CommandLinePlugin>();
        await plugins.RegisterAsync<ILoggingPlugin, LoggingPlugin>();
        await plugins.RegisterAsync<IConfigurationPlugin, ConfigurationPlugin>();
        await plugins.RegisterAsync<IGitPlugin, GitPlugin>();
        await plugins.RegisterAsync<INextVersionPlugin, NextVersionPlugin>();
        await plugins.RegisterAsync<VersionCalculationPerfomancePlugin>();

        if (pluginDescriptors is not null) {
            foreach (var pluginDescriptor in pluginDescriptors) {
                await plugins.RegisterAsync(pluginDescriptor.PluginType, pluginDescriptor.Plugin);
            }
        }

        var pluginEvents = new PluginEventCache();

        var pluginExecutor = new PluginExecutor(plugins, pluginEvents);
        await pluginExecutor.ExecuteAsync();

        int exitCode = (int)ExitCode.NotExecuted;
        using var exitCodeSubscription = pluginEvents.SubscribeOnce(CommandLineEvents.InvokedRootCommand, i => exitCode = i);

        pluginEvents.Publish(CommandLineEvents.SetCommandLineArgs, args);
        pluginEvents.Publish(CommandLineEvents.ParseCommandLineArgs);

        if (exitCode == (int)ExitCode.NotExecuted) {
            pluginEvents.Publish(LoggingEvents.EnableLoggingInfrastructure);
            pluginEvents.Publish(ConfigurationEvents.CreateConfiguration);
            pluginEvents.Publish(CommandLineEvents.InvokeRootCommand);
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
