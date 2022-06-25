using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Vernuntii.Plugins;
using Vernuntii.PluginSystem;

namespace Vernuntii.Console.GlobalTool;

/// <summary>
/// The program class.
/// </summary>
public static class ConsoleToolProgram
{
    /// <summary>
    /// Runs console application.
    /// </summary>
    /// <param name="args">arguments</param>
    /// <param name="pluginDescriptors"></param>
    /// <returns>exit code</returns>
    public static async Task<int> RunAsync(string[] args, IEnumerable<PluginDescriptor>? pluginDescriptors = null)
    {
        var pluginsToPrepend = new[] {
            PluginDescriptor.Create<ConsoleLocateCommandPlugin>(),
            PluginDescriptor.Create<MSBuildIntegrationCommandPlugin>(),
            PluginDescriptor.Create<MSBuildIntegrationLocateCommandPlugin>(),
        };

        await using var vernuntiiRunner = new VernuntiiRunner() {
            ConsoleArgs = args,
            PluginDescriptors = pluginDescriptors is null ? pluginsToPrepend : pluginsToPrepend.Concat(pluginDescriptors)
        };

        return await vernuntiiRunner.RunConsoleAsync();
    }

    /// <summary>
    /// Entry point of console application.
    /// </summary>
    /// <param name="args">arguments</param>
    /// <returns>exit code</returns>
    public static Task<int> Main(string[] args) => RunAsync(args);
}
