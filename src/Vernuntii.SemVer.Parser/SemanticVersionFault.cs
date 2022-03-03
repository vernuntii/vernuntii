namespace Vernuntii.SemVer.Parser
{
    /// <summary>
    /// Represents a fault.
    /// </summary>
    public readonly struct SemanticVersionFault
    {
        /// <summary>
        /// Range where the fault occured.
        /// </summary>
        public Range Range { get; }

        /// <summary>
        /// Expectation to resolve fault.
        /// </summary>
        public IdentifierExpectation Expectation { get; }

        /// <summary>
        /// Creates a fault.
        /// </summary>
        /// <param name="expectation"></param>
        /// <param name="range"></param>
        public SemanticVersionFault(IdentifierExpectation expectation, Range range)
        {
            Range = range;
            Expectation = expectation;
        }
    }
}
