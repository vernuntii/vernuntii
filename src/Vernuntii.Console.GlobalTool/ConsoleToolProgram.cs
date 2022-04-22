using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Vernuntii.PluginSystem;

namespace Vernuntii.Console.GlobalTool;

/// <summary>
/// The program class.
/// </summary>
public static class ConsoleToolProgram
{
    private const string Vernuntii = nameof(Vernuntii);

    private const string VernuntiiMSBuild = $"{Vernuntii}.{nameof(MSBuild)}";
    private const string MSBuildProps = $"{VernuntiiMSBuild}.props";
    private const string MSBuildTargets = $"{VernuntiiMSBuild}.targets";
    private const string MSBuildDll = $"{VernuntiiMSBuild}.dll";

    private const string VernuntiiConsole = $"{Vernuntii}.{nameof(Console)}";
    private const string ConsoleDll = $"{VernuntiiConsole}.dll";
    private const string ConsoleExe = $"{VernuntiiConsole}.exe";

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
                    FileLocation.MSBuildProps => Path.Combine(assemblyDirectory, BuildDirectory, MSBuildProps),
                    FileLocation.MSBuildTargets => Path.Combine(assemblyDirectory, BuildDirectory, MSBuildTargets),
                    FileLocation.MSBuildDll => Path.Combine(assemblyDirectory, MSBuildDll),
                    FileLocation.ConsoleDll => Path.Combine(assemblyDirectory, ConsoleDll),
                    FileLocation.ConsoleExe => Path.Combine(assemblyDirectory, ConsoleExe),
                    _ => throw new ArgumentException("The location you specified does not exist")
                };

                System.Console.WriteLine(filePath);
                return 0;
            });
        }

        protected override void OnCompletedRegistration() =>
            PluginRegistry.First<ICommandLinePlugin>().Registered +=
                plugin => plugin.RootCommand.AddCommand(fileLocationCommand);
    }
}
