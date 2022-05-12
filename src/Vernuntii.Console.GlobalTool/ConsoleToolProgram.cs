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
    private const string Vernuntii = nameof(Vernuntii);

    private const string VernuntiiConsoleMSBuild = $"{Vernuntii}.{nameof(Console)}.{nameof(MSBuild)}";
    private const string VernuntiiConsoleMSBuildProps = $"{VernuntiiConsoleMSBuild}.props";
    private const string VernuntiiConsoleMSBuildTargets = $"{VernuntiiConsoleMSBuild}.targets";
    private const string VernuntiiConsoleMSBuildDll = $"{VernuntiiConsoleMSBuild}.dll";

    private const string VernuntiiConsoleGlobalTool = $"{Vernuntii}.{nameof(Console)}.{nameof(GlobalTool)}";
    private const string VernuntiiConsoleGlobalToolDll = $"{VernuntiiConsoleGlobalTool}.dll";
    private const string VernuntiiConsoleGlobalToolExe = $"{VernuntiiConsoleGlobalTool}.exe";

    private const string BuildDirectory = "build";

    /// <summary>
    /// Runs console application.
    /// </summary>
    /// <param name="args">arguments</param>
    /// <param name="pluginDescriptors"></param>
    /// <returns>exit code</returns>
    public static Task<int> RunAsync(string[] args, IEnumerable<PluginDescriptor>? pluginDescriptors = null)
    {
        var pluginDescriptor = PluginDescriptor.Create<FileLocationPlugin>();
        return ConsoleProgram.RunAsync(args, pluginDescriptors?.Prepend(pluginDescriptor) ?? new[] { pluginDescriptor });
    }

    /// <summary>
    /// Entry point of console application.
    /// </summary>
    /// <param name="args">arguments</param>
    /// <returns>exit code</returns>
    public static Task<int> Main(string[] args) => RunAsync(args);

    private enum FileLocation
    {
        MSBuildProps = 1,
        MSBuildTargets = 2,
        MSBuildDll = 3,
        ConsoleExe = 4,
        ConsoleDll = 5
    }

    private class FileLocationPlugin : Plugin
    {
        private Argument<FileLocation> fileLocationArgument = new Argument<FileLocation>("location") {
            Description = "The file location you are asking for."
        };

        private Command fileLocationCommand;

        public FileLocationPlugin()
        {
            fileLocationCommand = new Command("location") {
                fileLocationArgument
            };

            // Parameter names are bound to naming convention, do not change!
            fileLocationCommand.Handler = CommandHandler.Create((FileLocation location) => {
                var assemblyDirectory = AppContext.BaseDirectory;

                var filePath = location switch {
                    FileLocation.MSBuildProps => Path.Combine(assemblyDirectory, BuildDirectory, VernuntiiConsoleMSBuildProps),
                    FileLocation.MSBuildTargets => Path.Combine(assemblyDirectory, BuildDirectory, VernuntiiConsoleMSBuildTargets),
                    FileLocation.MSBuildDll => Path.Combine(assemblyDirectory, VernuntiiConsoleMSBuildDll),
                    FileLocation.ConsoleDll => Path.Combine(assemblyDirectory, VernuntiiConsoleGlobalToolDll),
                    FileLocation.ConsoleExe => Path.Combine(assemblyDirectory, VernuntiiConsoleGlobalToolExe),
                    _ => throw new ArgumentException("The location you specified does not exist")
                };

                System.Console.WriteLine(filePath);
                return 0;
            });
        }

        protected override void OnCompletedRegistration() =>
            Plugins.First<ICommandLinePlugin>().Registered +=
                plugin => plugin.RootCommand.AddCommand(fileLocationCommand);
    }
}
