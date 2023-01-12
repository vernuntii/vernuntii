using Vernuntii.PluginSystem;
using Vernuntii.Runner;

namespace Vernuntii.Plugins;

/// <summary>
/// Similiar to <see cref="NextVersionPlugin"/> but daemonized. Must appear after <see cref="NextVersionPlugin"/>.
/// </summary>
internal class NextVersionDaemonPlugin : Plugin
{
    private readonly VernuntiiRunner _runner;

    public NextVersionDaemonPlugin(VernuntiiRunner runner, ICommandLinePlugin commandLine)
    {
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        //commandLine.RootCommand.Add(,
    }

    protected override void OnExecution()
    {

    }
}
