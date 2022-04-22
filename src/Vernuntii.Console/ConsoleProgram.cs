using System.CommandLine;
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
    public static Task<int> RunAsync(string[] args, IEnumerable<PluginDescriptor>? pluginDescriptors = null)
    {
        using var pluginRegistry = new PluginRegistry();

        pluginRegistry.Register<ICommandLinePlugin, CommandLinePlugin>();
        pluginRegistry.Register<ILoggingPlugin, LoggingPlugin>();
        pluginRegistry.Register<IGitPlugin, GitPlugin>();
        pluginRegistry.Register<INextVersionPlugin, NextVersionPlugin>();
        pluginRegistry.Register<VersionCalculationPerfomancePlugin>();

        if (pluginDescriptors is not null) {
            foreach (var pluginDescriptor in pluginDescriptors) {
                pluginRegistry.Register(pluginDescriptor.PluginType, pluginDescriptor.Plugin);
            }
        }

        var pluginEventAggregator = new PluginEventAggregator();

        var pluginExecutor = new PluginExecutor(pluginRegistry, pluginEventAggregator);
        pluginExecutor.Execute();

        int exitCode = (int)ExitCode.NotExecuted;
        using var exitCodeSubscription = pluginEventAggregator.GetEvent<CommandLineEvents.InvokedRootCommandEvent>().Subscribe(i => exitCode = i);

        pluginEventAggregator.PublishEvent(CommandLineEvents.SetCommandLineArgsEvent.Discriminator, args);
        pluginEventAggregator.PublishEvent<CommandLineEvents.ParseCommandLineArgsEvent>();

        if (exitCode == (int)ExitCode.NotExecuted) {
            pluginEventAggregator.PublishEvent<LoggingEvents.EnableLoggingInfrastructureEvent>();
            pluginEventAggregator.PublishEvent<CommandLineEvents.InvokeRootCommandEvent>();
        }

        pluginExecutor.Destroy();

        if (exitCode == (int)ExitCode.NotExecuted) {
            throw new InvalidOperationException("The command line plugin was not running");
        }

        return Task.FromResult(exitCode);
    }

    /// <summary>
    /// Entry point of console application.
    /// </summary>
    /// <param name="args"></param>
    /// <returns>exit code</returns>
    public static Task<int> Main(string[] args) => RunAsync(args);
}
