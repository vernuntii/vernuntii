namespace Vernuntii.Logging
{
    /// <summary>
    /// A amendment for a logger message.
    /// </summary>
    public record LoggerMessageAmendment
    {
        /// <summary>
        /// The format string with placeholders.
        /// </summary>
        public string FormatString { get; }
        /// <summary>
        /// The variables that are going to be placed into format string.
        /// </summary>
        public object[] Arguments { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="formatString"></param>
        /// <param name="arguments"></param>
        public LoggerMessageAmendment(string formatString, params object[] arguments)
        {
            FormatString = formatString;
            Arguments = arguments;
        }
    }
}
