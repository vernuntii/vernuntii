using System.CommandLine;
using System.CommandLine.Invocation;

namespace Vernuntii.Plugins.CommandLine
{
    internal class CommandWrapper : ICommandWrapper
    {
        public ICommandHandler? Handler => _command.Handler;

        private readonly Command _command;
        private readonly Func<Func<Task<int>>?, ICommandHandler?> _handlerFactory;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="handlerFactory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandWrapper(Command command, Func<Func<Task<int>>?, ICommandHandler?> handlerFactory)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        public ICommandHandler? SetHandler(Func<Task<int>>? handlerFunc)
        {
            var handler = _handlerFactory(handlerFunc);
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
