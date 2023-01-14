using System.CommandLine;

namespace Vernuntii.Plugins.CommandLine;

/// <summary>
/// Represents an extensible command to which only sub-commands, options or arguments can be added.
/// </summary>
public interface IExtensibleCommand
{
    /// <summary>
    /// Specifies if the command is read-only.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Adds an argument to the command.
    /// </summary>
    /// <param name="argument"></param>
    void Add(Argument argument);

    /// <summary>
    /// Adds a command to the command.
    /// </summary>
    /// <param name="command"></param>
    void Add(Command command);

    /// <summary>
    /// Adds an option to the command.
    /// </summary>
    /// <param name="option"></param>
    void Add(Option option);
}
