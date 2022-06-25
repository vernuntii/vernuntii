using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vernuntii.Plugins.CommandLine
{
    internal class CommandWrapper : ICommandWrapper
    {
        public Func<int>? HandlerFunc {
            get => _handlerFunc;
        }

        public ICommandHandler? Handler {
            get => _handler;
        }

        private Func<int>? _handlerFunc;
        private ICommandHandler? _handler;

        private readonly Command _command;
        private readonly Func<Func<int>?, ICommandHandler?> _handlerFactory;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="handlerFactory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandWrapper(Command command, Func<Func<int>?, ICommandHandler?> handlerFactory)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        public ICommandHandler? SetHandler(Func<int>? handlerFunc)
        {
            _handlerFunc = handlerFunc;
            var handler = _handlerFactory(handlerFunc);
            _command.Handler = handler;
            _handler = handler;
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
