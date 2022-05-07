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
    public sealed class IdentifierParseResult<T> : IIdentifierParseResult<T>, INullableIdentifierParseResult<T>, INotNullableIdentifierParseResult<T>
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

        /// <inheritdoc/>
        public IdentifierParseResultState State { get; }

        /// <inheritdoc/>
        public bool Suceeded => ((IdentifierParseResultState)int.MaxValue & (~_successFlags) & State) == 0;

        /// <inheritdoc/>
        public bool Failed => (_successFlags & State) == 0;

        /// <inheritdoc/>
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
    }
}
