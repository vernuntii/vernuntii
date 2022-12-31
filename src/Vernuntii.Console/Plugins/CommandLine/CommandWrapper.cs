using System.CommandLine;
using System.CommandLine.Invocation;

namespace Vernuntii.Plugins.CommandLine
{
    internal class CommandWrapper : ICommandWrapper
    {
        public ICommandHandler? Handler => _command.Handler;

        private readonly Command _command;
        private readonly Func<Func<Task<int>>?, ICommandHandler?> _commandHandlerFactory;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandHandlerFactory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandWrapper(Command command, Func<Func<Task<int>>?, ICommandHandler?> commandHandlerFactory)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        }

        public ICommandHandler? SetHandler(Func<Task<int>>? commandHandler)
        {
            var handler = _commandHandlerFactory(commandHandler);
            _command.Handler = handler;
            return handler;
        }

        public void Add(Argument commandArgument) =>
            _command.Add(commandArgument);

        public void Add(Command command) =>
            _command.Add(command);

        public void Add(Option commandOption) =>
            _command.Add(commandOption);
    }
}
