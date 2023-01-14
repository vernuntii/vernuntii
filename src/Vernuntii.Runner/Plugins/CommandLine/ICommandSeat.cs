namespace Vernuntii.Plugins.CommandLine;

/// <summary>
/// A command seat describes a command with sub-commands, options and arguments, that are used when the "seat" is taken.
/// </summary>
public interface ICommandSeat : ICommand
{
    /// <summary>
    /// Determines whether the seat is taken.
    /// </summary>
    bool IsSeatTaken { get; }
}
