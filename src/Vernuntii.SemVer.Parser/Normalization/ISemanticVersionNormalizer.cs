namespace Vernuntii.SemVer.Parser.Normalization
{
    /// <summary>
    /// Normalizes parts of a SemVer-compatible version string.
    /// </summary>
    public interface ISemanticVersionNormalizer
    {
        /// <summary>
        /// If true, then two or more adjacent dots are trimmed to one dot.
        /// </summary>
        bool TrimPreReleaseDots { get; }

        /// <summary>
        /// Normalizes <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value in which <paramref name="faults"/> are found.</param>
        /// <param name="faults">List of non-overlapping faults within <paramref name="value"/>.</param>
        /// <returns>Normalized version of <paramref name="value"/>.</returns>
        ReadOnlySpan<char> NormalizeFaults(ReadOnlySpan<char> value, IReadOnlyList<SemanticVersionFault> faults);
    }
}
