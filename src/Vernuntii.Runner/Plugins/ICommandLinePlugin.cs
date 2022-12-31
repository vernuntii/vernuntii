using Vernuntii.Plugins.CommandLine;
using Vernuntii.PluginSystem;

namespace Vernuntii.Plugins;

/// <summary>
/// The commandline plugin.
/// </summary>
public interface ICommandLinePlugin : IPlugin
{
    /// <summary>
    /// The root command.
    /// </summary>
    ICommandWrapper RootCommand { get; }

    /// <summary>
    /// If <see langword="true"/> and an exception has been thrown during command invocation, then exception will be rethrown and the (bad) exit code won't not returned.
    /// </summary>
    internal bool PreferExceptionOverExitCode { get; set; }
}
