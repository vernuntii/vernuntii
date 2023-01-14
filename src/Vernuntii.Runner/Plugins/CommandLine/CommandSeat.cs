using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vernuntii.Plugins.CommandLine;

/// <inheritdoc cref="ICommandSeat"/>
internal sealed class CommandSeat : ICommandSeat
{
    public bool IsReadOnly => _command.IsReadOnly;
    public bool IsSeatTaken => IsReadOnly || !ReferenceEquals(_command, _commandSeat);

    CommandSeatImpl _commandSeat;
    ICommand _command;

    public CommandSeat()
    {
        _commandSeat = new CommandSeatImpl();
        _command = _commandSeat;
    }

    /// <summary>
    /// Sets the command handlerFunc.
    /// </summary>
    /// <param name="handlerFunc"></param>
    public void SetHandler(Func<Task<int>>? handlerFunc)
    {
        lock (_commandSeat) {
            _command.SetHandler(handlerFunc);
        }
    }

    /// <summary>
    /// Adds a sub-command to the command.
    /// </summary>
    /// <param name="subCommand"></param>
    public void Add(Command subCommand)
    {
        lock (_commandSeat) {
            _command.Add(subCommand);
        }
    }

    /// <summary>
    /// Adds an argument to the command.
    /// </summary>
    /// <param name="argument"></param>
    public void Add(Argument argument)
    {
        lock (_commandSeat) {
            _command.Add(argument);
        }
    }

    /// <summary>
    /// Adds an option to the command.
    /// </summary>
    /// <param name="option"></param>
    public void Add(Option option)
    {
        lock (_commandSeat) {
            _command.Add(option);
        }
    }

    internal void TakeSeat(ICommand seatTakingCommand)
    {
        lock (_commandSeat) {
            _command = seatTakingCommand;
            _commandSeat.TakeSeat(seatTakingCommand);
        }
    }

    private class CommandSeatImpl : ICommand
    {
        public bool IsReadOnly => false;

        private List<Action<ICommand>>? _configureCommandActions;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MemberNotNull(nameof(_configureCommandActions))]
        private void InitializeCommandActionList() =>
            _configureCommandActions ??= new();

        /// <summary>
        /// Sets the command handlerFunc.
        /// </summary>
        /// <param name="handlerFunc"></param>
        public void SetHandler(Func<Task<int>>? handlerFunc)
        {
            InitializeCommandActionList();
            _configureCommandActions.Add(command => command.SetHandler(handlerFunc));
        }

        /// <summary>
        /// Adds a sub-command to the command.
        /// </summary>
        /// <param name="subCommand"></param>
        public void Add(Command subCommand)
        {
            InitializeCommandActionList();
            _configureCommandActions.Add(command => command.Add(subCommand));
        }

        /// <summary>
        /// Adds an argument to the command.
        /// </summary>
        /// <param name="argument"></param>
        public void Add(Argument argument)
        {
            InitializeCommandActionList();
            _configureCommandActions.Add(command => command.Add(argument));
        }

        /// <summary>
        /// Adds an option to the command.
        /// </summary>
        /// <param name="option"></param>
        public void Add(Option option)
        {
            InitializeCommandActionList();
            _configureCommandActions.Add(command => command.Add(option));
        }

        internal void TakeSeat(ICommand command)
        {
            if (_configureCommandActions is null) {
                return;
            }

            foreach (var configureCommand in _configureCommandActions) {
                configureCommand.Invoke(command);
            }
        }
    }
}
