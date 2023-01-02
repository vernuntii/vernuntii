using System.CommandLine;
using System.CommandLine.Invocation;

namespace Vernuntii.Plugins.CommandLine;

/// <summary>
/// A wrapper for <see cref="Command"/>.
/// </summary>
public interface IReadOnlyCommand
{
    /// <summary>
    /// The command handlerFunc.
    /// </summary>
    ICommandHandler? Handler { get; }
}
