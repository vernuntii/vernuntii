namespace Vernuntii.SemVer
{
    /// <inheritdoc/>
    public class SemanticVersionBuilderException : ArgumentException
    {
        /// <inheritdoc/>
        public SemanticVersionBuilderException() : base()
        {
        }

        /// <inheritdoc/>
        public SemanticVersionBuilderException(string? message) : base(message)
        {
        }

        /// <inheritdoc/>
        public SemanticVersionBuilderException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        public SemanticVersionBuilderException(string? message, string? paramName) : base(message, paramName)
        {
        }

        /// <inheritdoc/>
        public SemanticVersionBuilderException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException)
        {
        }
    }
}
