using System.CommandLine;
using System.CommandLine.Invocation;

namespace Vernuntii.Plugins.CommandLine
{
    /// <summary>
    /// A wrapper for <see cref="Command"/>.
    /// </summary>
    public interface ICommandWrapper
    {
        /// <summary>
        /// The internal command handlerFunc.
        /// </summary>
        Func<int>? HandlerFunc { get; }

        /// <summary>
        /// The command handlerFunc.
        /// </summary>
        ICommandHandler? Handler { get; }

        /// <summary>
        /// Sets the command handlerFunc.
        /// </summary>
        /// <param name="handlerFunc"></param>
        /// <returns></returns>
        ICommandHandler? SetHandler(Func<int>? handlerFunc);

        /// <summary>
        /// Adds an commandArgument to command.
        /// </summary>
        /// <param name="commandArgument"></param>
        void Add(Argument commandArgument);

        /// <summary>
        /// Adds a subcommand. 
        /// </summary>
        /// <param name="command"></param>
        void Add(Command command);

        /// <summary>
        /// Adds an commandOption to command.
        /// </summary>
        /// <param name="commandOption"></param>
        void Add(Option commandOption);
    }
}
