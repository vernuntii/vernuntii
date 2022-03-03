using Vernuntii.SemVer.Parser.Extensions;

namespace Vernuntii.SemVer.Parser.Normalization
{
    /// <summary>
    /// Provides pre-defined instances of type <see cref="ISemanticVersionNormalizer"/>.
    /// </summary>
    public static class SemanticVersionNormalizer
    {
        internal delegate void FixFaultAction(Span<char> value, SemanticVersionFault fault);

        /// <summary>
        /// Removes partial or fully the content depending on expectation and range.
        /// </summary>
        public readonly static ISemanticVersionNormalizer Erase = new FixingSemanticVersionNormalizer(FixFaultByErasing);
        /// <summary>
        /// No normalization is happening, so output is equal to input.
        /// </summary>
        public readonly static ISemanticVersionNormalizer NoAction = new NoActionSemanticVersionNormalizer();

        private static void FixFaultByErasing(Span<char> value, SemanticVersionFault fault)
        {
            if (fault.Expectation == IdentifierExpectation.SingleZero) {
                value[fault.Range.WithStart(1)].Clear();
            } else if (fault.Expectation == IdentifierExpectation.Alphanumeric
                || fault.Expectation == IdentifierExpectation.Numeric) {
                value[fault.Range].Clear();
            }
        }

        private class FixingSemanticVersionNormalizer : ISemanticVersionNormalizer
        {
            public bool TrimPreReleaseDots => true;

            private FixFaultAction _fixFault;

            internal FixingSemanticVersionNormalizer(FixFaultAction fixFault) =>
                _fixFault = fixFault;

            /// <inheritdoc/>
            public ReadOnlySpan<char> NormalizeFaults(ReadOnlySpan<char> value, IReadOnlyList<SemanticVersionFault> faults) =>
                string.Create(value.Length, faults, (preReleaseIdentifierSpan, faults) => {
                    for (int i = faults.Count - 1; i >= 0; i--) {
                        _fixFault(preReleaseIdentifierSpan, faults[i]);
                    }
                });
        }

        private class NoActionSemanticVersionNormalizer : ISemanticVersionNormalizer
        {
            public bool TrimPreReleaseDots => false;

            public ReadOnlySpan<char> NormalizeFaults(ReadOnlySpan<char> value, IReadOnlyList<SemanticVersionFault> faults) =>
                value;
        }
    }
}
