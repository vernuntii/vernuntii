namespace Vernuntii.SemVer.Parser
{
    /// <summary>
    /// Represents the state of <see cref="IdentifierParseResult"/>.
    /// </summary>
    public enum IdentifierParseResultState
    {
        /// <summary>
        /// Result is equal to <see langword="null"/>.
        /// </summary>
        Null = 1,
        /// <summary>
        /// Invalid because empty.
        /// </summary>
        Empty = 2,
        /// <summary>
        /// Invalid because containing white-space.
        /// </summary>
        WhiteSpace = 4,
        /// <summary>
        /// Invalid because parse failed.
        /// </summary>
        InvalidParse = 8,
        /// <summary>
        /// Valid state.
        /// </summary>
        ValidParse = 16,
        /// <summary>
        /// Successful states.
        /// </summary>
        Success = ValidParse
    }
}
