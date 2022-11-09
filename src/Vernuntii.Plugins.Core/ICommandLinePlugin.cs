using System.CommandLine;
using System.CommandLine.Builder;
using Microsoft.Extensions.Logging;
using Vernuntii.Plugins.CommandLine;
using Vernuntii.PluginSystem.Lifecycle;

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
    /// A delegate that creates an instance of <see cref="CommandLineBuilder"/>.
    /// </summary>
    Action<CommandLineBuilder, ILogger> ConfigureCommandLineBuilderAction { get; set; }
}
