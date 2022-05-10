using Microsoft.Extensions.Logging;
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

        var loggerPlugin = new LoggingPlugin();
        await plugins.RegisterAsync<ILoggingPlugin>(loggerPlugin);
        ILogger logger = loggerPlugin.CreateLogger(nameof(ConsoleProgram));

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
        logger.LogTrace("Executing plugins");

        int exitCode = (int)ExitCode.NotExecuted;
        using var exitCodeSubscription = pluginEvents.SubscribeOnce(CommandLineEvents.InvokedRootCommand, i => exitCode = i);

        pluginEvents.Publish(CommandLineEvents.SetCommandLineArgs, args);
        logger.LogTrace("Set command-line arguments");

        pluginEvents.Publish(CommandLineEvents.ParseCommandLineArgs);
        logger.LogTrace("Parse command-line arguments");

        pluginEvents.Publish(LoggingEvents.EnableLoggingInfrastructure);
        logger.LogTrace("Enable logging infrastructure");

        pluginEvents.Publish(ConfigurationEvents.CreateConfiguration);
        logger.LogTrace("Create configuration");

        pluginEvents.Publish(CommandLineEvents.InvokeRootCommand);
        logger.LogTrace("Invoke command-line root command");

        logger.LogTrace("Destroying plugins");
        await pluginExecutor.DestroyAsync();
        logger.LogTrace("Destroyed plugins");

        if (exitCode == (int)ExitCode.NotExecuted) {
            throw new InvalidOperationException("The command line was not running");
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
