using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.SemVer.Parser
{
    /// <summary>
    /// Contains helper functions for <see cref="IdentifierParseResult{T}"/>.
    /// </summary>
    public static class IdentifierParseResult
    {
        /// <summary>
        /// Parse result with state <see cref="IdentifierParseResultState.InvalidParse"/>.
        /// </summary>
        public static IdentifierParseResult<T> InvalidParse<T>(T? value) => new IdentifierParseResult<T>(IdentifierParseResultState.InvalidParse, value);

        /// <summary>
        /// Parse result with state <see cref="IdentifierParseResultState.ValidParse"/>.
        /// </summary>
        public static IdentifierParseResult<T> ValidParse<T>(T? value) => new IdentifierParseResult<T>(IdentifierParseResultState.ValidParse, value);
    }

    /// <summary>
    /// Represents the parse result.
    /// </summary>
    public sealed class IdentifierParseResult<T>
    {
        /// <summary>
        /// Parse result with invalid state <see cref="IdentifierParseResultState.Null"/>.
        /// </summary>
        public readonly static IdentifierParseResult<T> InvalidNull = new IdentifierParseResult<T>(IdentifierParseResultState.Null);

        /// <summary>
        /// Parse result with valid state <see cref="IdentifierParseResultState.Null"/>.
        /// </summary>
        public readonly static IdentifierParseResult<T> ValidNull = new IdentifierParseResult<T>(IdentifierParseResultState.Null, IdentifierParseResultState.Null);

        /// <summary>
        /// Parse result with state <see cref="IdentifierParseResultState.Empty"/>.
        /// </summary>
        public readonly static IdentifierParseResult<T> InvalidEmpty = new IdentifierParseResult<T>(IdentifierParseResultState.Empty);

        /// <summary>
        /// Parse result with state <see cref="IdentifierParseResultState.WhiteSpace"/>.
        /// </summary>
        public readonly static IdentifierParseResult<T> InvalidWhiteSpace = new IdentifierParseResult<T>(IdentifierParseResultState.WhiteSpace);

        /// <summary>
        /// Represents the state of parse result.
        /// </summary>
        public IdentifierParseResultState State { get; }

        /// <summary>
        /// True if state contains only success states.
        /// </summary>
        public bool Suceeded => ((IdentifierParseResultState)int.MaxValue & (~_successFlags) & State) == 0;

        /// <summary>
        /// True if state contains only failure states.
        /// </summary>
        public bool Failed => (_successFlags & State) == 0;

        /// <summary>
        /// Represents the value of parse result.
        /// </summary>
        public T? Value { get; init; }

        private IdentifierParseResultState _successFlags;

        internal IdentifierParseResult(IdentifierParseResultState state, IdentifierParseResultState successFlags)
        {
            State = state;
            _successFlags = successFlags;
            CheckState();
        }

        internal IdentifierParseResult(IdentifierParseResultState state, T? value)
        {
            _successFlags = IdentifierParseResultState.Success;
            State = state;
            Value = value;
            CheckState();
        }

        internal IdentifierParseResult(IdentifierParseResultState state)
            : this(state, value: default) { }

        private void CheckState()
        {
            if (Suceeded == Failed) {
                throw new InvalidOperationException("State can either only contain successes or failures but not both");
            }
        }

        /// <summary>
        /// Checks if state has any successful state.
        /// </summary>
        /// <param name="value"></param>
        public bool DeconstructSuccess([NotNullWhen(true)] out T? value)
        {
            value = Value;

            if (Suceeded) {
#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
                return true;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
            }

            return false;
        }

        /// <summary>
        /// Checks if state has any successful state.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        public bool DeconstructSuccess([NotNullIfNotNull("defaultValue")] out T? value, T defaultValue)
        {
            if (Value is null) {
                value = defaultValue;
            } else {
                value = Value;
            }

            if (Failed) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if state has any failure.
        /// </summary>
        /// <param name="value"></param>
        public bool DeconstructFailure([NotNullWhen(false)] out T? value)
        {
            value = Value;

            if (Failed) {
                return true;
            }

#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
            return false;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
        }

        /// <summary>
        /// Checks if state has any failure.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        public bool DeconstructFailure([NotNullIfNotNull("defaultValue")] out T? value, T defaultValue)
        {
            if (Value is null) {
                value = defaultValue;
            } else {
                value = Value;
            }

            if (Failed) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deconstructs this instance.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="value"></param>
        public void Deconstruct(out IdentifierParseResultState state, out T? value)
        {
            state = State;
            value = Value;
        }

        /// <summary>
        /// Deconstruct this instance.
        /// </summary>
        /// <param name="value"></param>
        public IdentifierParseResultState Deconstruct(out T? value)
        {
            value = Value;
            return State;
        }
    }
}
