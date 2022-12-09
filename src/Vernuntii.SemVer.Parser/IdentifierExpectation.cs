using Vernuntii.SemVer.Parser.Normalization;

namespace Vernuntii.SemVer.Parser
{
    /// <summary>
    /// Represents an expectation of an identifier.
    /// </summary>
    /// <remarks>
    /// Required for <see cref="ISemanticVersionNormalizer.NormalizeFaults(ReadOnlyMemory{char}, IReadOnlyList{SemanticVersionFault})"/>.
    /// </remarks>
    public enum IdentifierExpectation
    {
        /// <summary>
        /// Expect single zero.
        /// <code>
        /// ^0$
        /// </code>
        /// </summary>
        SingleZero = 1,
        /// <summary>
        /// Expect alphanumerics.
        /// <code>
        /// ^[a-zA-Z0-9-]+$
        /// </code>
        /// </summary>
        Alphanumeric = 2,
        /// <summary>
        /// Expect digits.
        /// <code>
        /// ^[0-9]+$
        /// </code>
        /// </summary>
        Numeric = 3,
        /// <summary>
        /// Expect empty.
        /// </summary>
        Empty = 4,
    }
}
