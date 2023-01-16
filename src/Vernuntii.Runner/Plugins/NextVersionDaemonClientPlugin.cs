using System.CommandLine;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins;

internal class NextVersionDaemonClientPlugin : Plugin
{
    public readonly INextVersionPlugin _nextVersionPlugin;

    private readonly Option<string[]> _daemonClientOption = new Option<string[]>("--daemon-client", $"Allows to start {nameof(Vernuntii)} as client.") {
        IsHidden = true
    };

    public NextVersionDaemonClientPlugin(INextVersionPlugin nextVersionPlugin)
    {
        _nextVersionPlugin = nextVersionPlugin;
        _nextVersionPlugin.Command.Add(_daemonClientOption);
    }

    protected override void OnExecution()
    {
        Events.Once(LifecycleEvents.BeforeEveryRun)
            .Zip(Events.Once(CommandLineEvents.ParsedCommandLineArguments))
            .Subscribe(result => {
                while (true) {
                    Console.ReadKey();
                }
            });
    }
}
