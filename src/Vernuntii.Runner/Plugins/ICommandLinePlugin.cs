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
    /// <remarks>
    /// To set the command handler you have to request a seat by calling <see cref="RequestRootCommandSeat"/>.
    /// </remarks>
    IExtensibleCommand RootCommand { get; }

    /// <summary>
    /// Creates a <b>new</b> command seat.
    /// </summary>
    /// <remarks>
    /// The last requested command seat is taken when the root command is about to be sealed.
    /// </remarks>
    ICommandSeat RequestRootCommandSeat();
}
