using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer.Parser.Extensions;
using Vernuntii.SemVer.Parser.Normalization;
using static Vernuntii.SemVer.Parser.SemanticVersionCharacters;

namespace Vernuntii.SemVer.Parser.Parsers
{
    internal class IdentifierParser
    {
        public delegate bool TryParseNonEmptyIdentifier<T>(string dottedIdentifier, [NotNullWhen(true)] out T? result);
        public delegate IReadOnlyList<SemanticVersionFault> SearchFaultsDelegate(ReadOnlySpan<char> spar);

        public readonly static IdentifierParser Strict = new IdentifierParser(SemanticVersionNormalizer.NoAction);

        public static IdentifierParseResult<T> TryParseIdentifier<T>(
            string? identifier,
            TryParseNonEmptyIdentifier<T> tryParse,
            bool allowNull = false)
        {
            if (identifier == null) {
                if (allowNull) {
                    return IdentifierParseResult<T>.ValidNull;
                } else {
                    return IdentifierParseResult<T>.InvalidNull;
                }
            } else if (identifier.Length == 0) {
                return IdentifierParseResult<T>.InvalidEmpty;
            } else if (string.IsNullOrWhiteSpace(identifier)) {
                return IdentifierParseResult<T>.InvalidWhiteSpace;
            } else if (!tryParse(identifier, out var preReleaseIdentifiers)) {
                return IdentifierParseResult.InvalidParse(preReleaseIdentifiers);
            } else {
                return IdentifierParseResult.ValidParse(preReleaseIdentifiers);
            }
        }

        private static bool IsContainedInAlphanumericIdentifierCharset(char character) =>
            character == Hyphen
            || (character >= A && character <= Z)
            || (character >= a && character <= z)
            || char.IsDigit(character);

        public static IReadOnlyList<SemanticVersionFault> SearchFaults(
            ReadOnlySpan<char> value,
            bool lookupSingleZero = false,
            bool lookupAlphanumeric = false,
            bool lookupNumeric = false)
        {
            var preReleaseIdentifierLength = value.Length;
            List<SemanticVersionFault> faults = new List<SemanticVersionFault>();

            if (lookupSingleZero && value.HasNumberLeadingZeros(out var leadingZeros)) {
                faults.Add(new SemanticVersionFault(IdentifierExpectation.SingleZero, 0..leadingZeros));
            } else {
                for (int i = 0; i < preReleaseIdentifierLength; i++) {
                    var currentCharacter = value[i];

                    if (lookupAlphanumeric && !IsContainedInAlphanumericIdentifierCharset(currentCharacter)) {
                        int faultStartAt = i;

                        for (int j = i + 1; j < preReleaseIdentifierLength; j++) {
                            currentCharacter = value[i];

                            if (IsContainedInAlphanumericIdentifierCharset(currentCharacter)) {
                                faults.Add(new SemanticVersionFault(IdentifierExpectation.Alphanumeric, faultStartAt..(j - 1)));
                                goto @continue;
                            }
                        }

                        faults.Add(new SemanticVersionFault(IdentifierExpectation.Alphanumeric, faultStartAt..preReleaseIdentifierLength));
                    }

                    if (lookupNumeric && !char.IsDigit(currentCharacter)) {
                        int faultStartAt = i;

                        for (int j = i + 1; j < preReleaseIdentifierLength; j++) {
                            currentCharacter = value[i];

                            if (char.IsDigit(currentCharacter)) {
                                faults.Add(new SemanticVersionFault(IdentifierExpectation.Numeric, faultStartAt..(j - 1)));
                                goto @continue;
                            }
                        }

                        faults.Add(new SemanticVersionFault(IdentifierExpectation.Numeric, faultStartAt..preReleaseIdentifierLength));
                    }

                    @continue:
                    ;
                }
            }

            return faults;
        }

        public ISemanticVersionNormalizer Normalizer { get; }

        public IdentifierParser(ISemanticVersionNormalizer normalizer) => Normalizer = normalizer;

        public bool TryResolveFaults(ReadOnlySpan<char> value, SearchFaultsDelegate searchFaults, out ReadOnlySpan<char> result)
        {
            recheck:
            var faults = searchFaults(value);

            if (faults.Count != 0) {
                var normalized = Normalizer.NormalizeFaults(value, faults);

                if (value.Equals(normalized, StringComparison.Ordinal)) {
                    goto exit;
                }

                value = normalized;
                goto recheck;
            }

            result = value;
            return true;

            exit:
            result = null;
            return false;
        }

        public bool TryParseDottedIdentifier(
            string dottedIdentifier,
            [NotNullWhen(true)] out IEnumerable<string>? dotSplittedIdentifiers,
            bool lookupSingleZero = false)
        {
            var dotSplittedIdentifierArray = dottedIdentifier.Split(Dot);
            var dotSplittedIdentifierArrayLength = dotSplittedIdentifierArray.Length;
            var emptyIdentifiers = 0;

            for (int i = 0; i < dotSplittedIdentifierArrayLength; i++) {
                ReadOnlySpan<char> unresolved = dotSplittedIdentifierArray[i];

                var success = TryResolveFaults(
                    unresolved,
                    value => SearchFaults(
                        value,
                        lookupSingleZero: lookupSingleZero,
                        lookupAlphanumeric: true),
                    out var resolved);

                if (!success) {
                    goto exit;
                }

                if (unresolved != resolved) {
                    dotSplittedIdentifierArray[i] = resolved.ToString();
                }

                if (dotSplittedIdentifierArray[i].Length == 0) {
                    emptyIdentifiers++;
                }
            }

            if (emptyIdentifiers == dotSplittedIdentifierArray.Length
                || (!Normalizer.TrimPreReleaseDots && emptyIdentifiers > 0)) {
                goto exit;
            }

            dotSplittedIdentifiers = new IdentifierEnumerable(dotSplittedIdentifierArray);
            return true;

            exit:
            dotSplittedIdentifiers = null;
            return false;
        }
    }
}
