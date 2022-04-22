using System.CommandLine;
using System.CommandLine.Builder;

namespace Vernuntii.PluginSystem;

/// <summary>
/// The commandline plugin.
/// </summary>
public interface ICommandLinePlugin : IPlugin
{
    /// <summary>
    /// A delegate that creates an instance of <see cref="CommandLineBuilder"/>.
    /// </summary>
    Func<RootCommand, CommandLineBuilder> CommandLineBuilderFactory { get; set; }
    /// <summary>
    /// The root command.
    /// </summary>
    RootCommand RootCommand { get; }

    /// <summary>
    /// Sets handler for <see cref="RootCommand"/>.
    /// </summary>
    /// <param name="action"></param>
    void SetRootCommandHandler(Func<int> action);
}
