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
        public static readonly ISemanticVersionNormalizer Erase = new FixingSemanticVersionNormalizer(FixFaultByErasing);

        /// <summary>
        /// No normalization is happening, so output is equal to input.
        /// </summary>
        public static readonly ISemanticVersionNormalizer NoAction = new NoActionSemanticVersionNormalizer();

        private static void FixFaultByErasing(Span<char> value, SemanticVersionFault fault)
        {
            if (fault.Expectation == IdentifierExpectation.SingleZero) {
                value[fault.Range.WithStart(1)].Clear();
            } else if (fault.Expectation == IdentifierExpectation.Alphanumeric
                || fault.Expectation == IdentifierExpectation.Numeric
                || fault.Expectation == IdentifierExpectation.Empty) {
                value[fault.Range].Clear();
            }
        }

        private class FixingSemanticVersionNormalizer : ISemanticVersionNormalizer
        {
            public bool TrimPreReleaseDots => true;

            private readonly FixFaultAction _fixFault;

            internal FixingSemanticVersionNormalizer(FixFaultAction fixFault) =>
                _fixFault = fixFault;

            /// <inheritdoc/>
            public ReadOnlyMemory<char> NormalizeFaults(SemanticVersionPart versionPart, ReadOnlyMemory<char> value, IReadOnlyList<SemanticVersionFault> faults)
            {
                var newLength = 0;

                return string.Create(value.Length, value, (newValue, value) => {
                    value.Span.CopyTo(newValue);

                    for (var i = faults.Count - 1; i >= 0; i--) {
                        _fixFault(newValue, faults[i]);
                    }

                    var newValueLength = newValue.Length;

                    for (var i = 0; i < newValueLength; i++) {
                        if (newValue[i] != '\0') {
                            if (newLength != i) {
                                newValue[newLength] = newValue[i];
                                newValue[i] = '\0';
                            }

                            newLength++;
                        }
                    }
                }).AsMemory(0, newLength);
            }
        }

        private class NoActionSemanticVersionNormalizer : ISemanticVersionNormalizer
        {
            public bool TrimPreReleaseDots => false;

            public ReadOnlyMemory<char> NormalizeFaults(SemanticVersionPart versionPart, ReadOnlyMemory<char> value, IReadOnlyList<SemanticVersionFault> faults) =>
                value;
        }
    }
}
