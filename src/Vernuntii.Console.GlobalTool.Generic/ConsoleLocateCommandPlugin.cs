using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Reflection;
using Vernuntii.Plugins;
using Vernuntii.PluginSystem;

namespace Vernuntii.Console.GlobalTool;

internal class ConsoleLocateCommandPlugin : Plugin
{
    private Argument<ConsoleFileLocation> locateArgument = new Argument<ConsoleFileLocation>("locate") {
        Description = "The file location you are asking for."
    };

    private Command locateCommand;

    public ConsoleLocateCommandPlugin()
    {
        locateCommand = new Command("locate") {
            locateArgument
        };

        // Parameter names are bound to naming convention, do not change!
        locateCommand.Handler = CommandHandler.Create((ConsoleFileLocation locate) => {
            var assemblyDirectory = AppContext.BaseDirectory;

            var entryAssemblyName = (Assembly.GetEntryAssembly()
                ?? throw new NotSupportedException("Entry assembly not available"))
                .GetName().Name;

            var filePath = locate switch {
                ConsoleFileLocation.ConsoleDll => Path.Combine(assemblyDirectory, $"{entryAssemblyName}.dll"),
                ConsoleFileLocation.ConsoleExe => Path.Combine(assemblyDirectory, $"{entryAssemblyName}.exe"),
                _ => throw new ArgumentException("The locate you specified does not exist")
            };

            System.Console.WriteLine(filePath);
            return 0;
        });
    }

    protected override void OnExecution() =>
        Plugins.GetPlugin<ICommandLinePlugin>().RootCommand.Add(locateCommand);
}
