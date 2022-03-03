namespace Vernuntii.SemVer.Parser.Slicing
{
    /// <summary>
    /// Default implementation for <see cref="IPrefixMajorSlicer"/>.
    /// </summary>
    public sealed class PrefixMajorSlicer : IPrefixMajorSlicer
    {
        /// <summary>
        /// Default instance.
        /// </summary>
        public readonly static PrefixMajorSlicer Default = new PrefixMajorSlicer();

        /// <inheritdoc/>
        public void Slice(ReadOnlySpan<char> value, out string prefix, out string major)
        {
            for (var i = value.Length - 1; i >= 0; i--) {
                if (!char.IsDigit(value[i])) {
                    prefix = value.Slice(0, i + 1).ToString();
                    major = value.Slice(i + 1).ToString();
                    return;
                }
            }

            prefix = string.Empty;
            major = value.ToString();
        }
    }
}
