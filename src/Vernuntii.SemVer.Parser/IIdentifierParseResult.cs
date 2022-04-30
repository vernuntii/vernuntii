namespace Vernuntii.SemVer.Parser
{
    /// <summary>
    /// Represents the parse result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IIdentifierParseResult<T>
    {
        /// <summary>
        /// Represents the state of parse result.
        /// </summary>
        IdentifierParseResultState State { get; }

        /// <summary>
        /// True if state contains only success states.
        /// </summary>
        bool Suceeded { get; }

        /// <summary>
        /// True if state contains only failure states.
        /// </summary>
        bool Failed { get; }

        /// <summary>
        /// Represents the value of parse result.
        /// </summary>
        T? Value { get; }
    }
}
