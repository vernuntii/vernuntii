using static Vernuntii.SemVer.Parser.SemanticVersionCharacters;

namespace Vernuntii.SemVer.Parser.Slicing
{
    /// <inheritdoc/>
    public sealed class SemanticVersionSlicer : ISemanticVersionSlicer
    {
        /// <summary>
        /// Default instance of <see cref="SemanticVersionSlicer"/>.
        /// </summary>
        public static readonly SemanticVersionSlicer Default = new(PrefixMajorSlicer.Default);

        private readonly IPrefixMajorSlicer _prefixMajorSlicer;

        /// <summary>
        /// Created an instance of this type.
        /// </summary>
        /// <param name="prefixMajorSlicer"></param>
        public SemanticVersionSlicer(IPrefixMajorSlicer prefixMajorSlicer) =>
            _prefixMajorSlicer = prefixMajorSlicer;

        /// <inheritdoc/>
        public bool TrySlice(string? value, out string? prefix, out string? major, out string? minor, out string? patch, out string? preRelease, out string? build)
        {
            if (value == null) {
                goto prefix;
            }

            // major
            var valueSpan = (ReadOnlySpan<char>)value;
            var firstDot = valueSpan.IndexOf(Dot);

            if (firstDot == -1) {
                _prefixMajorSlicer.Slice(value, out prefix, out major);
                goto minor;
            }

            var majorSpan = valueSpan.Slice(0, firstDot);
            _prefixMajorSlicer.Slice(majorSpan, out prefix, out major);

            // minor
            var afterMajorSpan = valueSpan.Slice(firstDot + 1);
            var secondDot = afterMajorSpan.IndexOf(Dot);

            if (secondDot == -1) {
                minor = afterMajorSpan.ToString();
                goto patch;
            }

            var minorSpan = afterMajorSpan.Slice(0, secondDot);
            minor = minorSpan.ToString();

            // prepare patch
            var afterMinorSpan = afterMajorSpan.Slice(secondDot + 1);
            var firstPlus = afterMinorSpan.IndexOf(Plus);
            ReadOnlySpan<char> beforePlusSpan;

            // build
            if (firstPlus == -1) {
                beforePlusSpan = afterMinorSpan;
                build = null;
            } else {
                beforePlusSpan = afterMinorSpan.Slice(0, firstPlus);
                build = afterMinorSpan.Slice(firstPlus + 1).ToString();
            }

            var firstHyphen = beforePlusSpan.IndexOf(Hyphen);

            // patch
            // pre-release
            if (firstHyphen == -1) {
                patch = beforePlusSpan.ToString();
                preRelease = null;
            } else {
                patch = beforePlusSpan.Slice(0, firstHyphen).ToString();
                preRelease = beforePlusSpan.Slice(firstHyphen + 1).ToString();
            }

            return true;

            prefix:
            prefix = null;
            major = null;
            minor:
            minor = null;
            patch:
            patch = null;
            preRelease = null;
            build = null;
            return false;
        }
    }
}
