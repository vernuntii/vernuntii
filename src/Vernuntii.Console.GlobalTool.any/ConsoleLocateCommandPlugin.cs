using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Reflection;
using Vernuntii.Plugins;
using Vernuntii.PluginSystem;

namespace Vernuntii.Console.GlobalTool;

internal class ConsoleLocateCommandPlugin : Plugin
{
    private readonly ICommandLinePlugin _commandLine;

    private readonly Argument<ConsoleFileLocation> _locateArgument = new("locate") {
        Description = "The file location you are asking for."
    };

    private readonly Command _locateCommand;

    public ConsoleLocateCommandPlugin(ICommandLinePlugin commandLine)
    {
        _locateCommand = new Command("locate") {
            _locateArgument
        };

        // Parameter names are bound to naming convention, do not change!
        _locateCommand.Handler = CommandHandler.Create((ConsoleFileLocation locate) => {
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
        _commandLine = commandLine;
    }

    protected override void OnExecution() =>
        _commandLine.RootCommand.Add(_locateCommand);
}
