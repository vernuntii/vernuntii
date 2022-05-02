using static Vernuntii.SemVer.Parser.SemanticVersionCharacters;

namespace Vernuntii.SemVer.Parser.Extensions
{
    internal static class ReadOnlySpanCharExtensions
    {
        public static bool IsNumeric(this ReadOnlySpan<char> value, out int zeros)
        {
            zeros = 0;
            bool canIncreaseLeadingZero = true;
            var preReleaseLength = value.Length;

            for (int i = 0; i < preReleaseLength; i++) {
                var currentCharacter = value[i];

                if (!char.IsDigit(currentCharacter)) {
                    return false;
                }

                if (canIncreaseLeadingZero && currentCharacter == Zero) {
                    zeros++;
                } else {
                    canIncreaseLeadingZero = false;
                }
            }

            return true;
        }

        public static bool HasNumberLeadingZeros(this ReadOnlySpan<char> value, out int zeros)
        {
            if (value.Length >= 2
                && value[0] == Zero
                && value.Slice(1).IsNumeric(out var otherZeros)) {
                zeros = otherZeros + 1;
                return true;
            }

            zeros = 0;
            return false;
        }
    }
}
