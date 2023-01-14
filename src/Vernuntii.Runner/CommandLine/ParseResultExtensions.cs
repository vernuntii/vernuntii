using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace Vernuntii.CommandLine;

internal static class ParseResultExtensions
{
    /// <summary>
    /// Checks if the command handler of parse result is equivalent to <paramref name="commandHandler"/>.
    /// </summary>
    /// <param name="parseResult"></param>
    /// <param name="commandHandler"></param>
    public static bool IsCommandHandlerEquivalentTo(this ParseResult parseResult, ICommandHandler? commandHandler) =>
            parseResult.CommandResult.Command.Handler == commandHandler;

    /// <summary>
    /// Checks if the command handler of parse result is not equivalent to <paramref name="commandHandler"/>.
    /// </summary>
    /// <param name="parseResult"></param>
    /// <param name="commandHandler"></param>
    public static bool IsCommandHandlerNotEquivalentTo(this ParseResult parseResult, ICommandHandler? commandHandler) =>
            parseResult.CommandResult.Command.Handler != commandHandler;
}
