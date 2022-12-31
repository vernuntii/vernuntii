using Vernuntii.PluginSystem;
using Vernuntii.Runner;

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
    /// <param name="configureBuilder"></param>
    /// <returns>exit code</returns>
    public static async Task<int> RunAsync(string[] args, Action<IVernuntiiRunnerBuilder>? configureBuilder = null)
    {
        var vernuntiiRunnerBuilder = VernuntiiRunnerBuilder
            .ForNextVersion()
            .ConfigurePlugins(plugins => {
                plugins.Add(PluginDescriptor.Create<ConsoleLocateCommandPlugin>());
                plugins.Add(PluginDescriptor.Create<MSBuildIntegrationCommandPlugin>());
                plugins.Add(PluginDescriptor.Create<MSBuildIntegrationLocateCommandPlugin>());
            });

        configureBuilder?.Invoke(vernuntiiRunnerBuilder);
        await using var vernuntiiRunner = vernuntiiRunnerBuilder.Build(args);
        return await vernuntiiRunner.RunAsync();
    }

    /// <summary>
    /// Entry point of console application.
    /// </summary>
    /// <param name="args">arguments</param>
    /// <returns>exit code</returns>
    public static Task<int> Main(string[] args) => RunAsync(args);
}
