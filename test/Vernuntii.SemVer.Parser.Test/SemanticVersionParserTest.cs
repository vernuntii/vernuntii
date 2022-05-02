using Vernuntii.SemVer.Parser.Normalization;
using Vernuntii.SemVer.Parser.Parsers;
using Xunit;

namespace Vernuntii.SemVer.Parser
{
    public class SemanticVersionParserTest
    {
        private static IEnumerable<object?[]> TryParseShouldParseStrictSucceededGenerator(SemanticVersionParser parser)
        {
            yield return new object?[] { parser, "0.0.0", new ParseResult(true, "", 0, 0, 0) };

            yield return new object?[] { parser, "v0.0.0", new ParseResult(true, "v", 0, 0, 0) };

            yield return new object?[] { parser, "0.0.0-a", new ParseResult(true, "", 0, 0, 0, new[] { "a" }) };
            yield return new object?[] { parser, "0.0.0-a.b", new ParseResult(true, "", 0, 0, 0, new[] { "a", "b" }) };
            yield return new object?[] { parser, "0.0.0-00a", new ParseResult(true, "", 0, 0, 0, new[] { "00a" }) };
            yield return new object?[] { parser, "0.0.0-0.a", new ParseResult(true, "", 0, 0, 0, new[] { "0", "a" }) };

            yield return new object?[] { parser, "0.0.0+a", new ParseResult(true, "", 0, 0, 0, null, new[] { "a" }) };
            yield return new object?[] { parser, "0.0.0+a.b", new ParseResult(true, "", 0, 0, 0, null, new[] { "a", "b" }) };
            yield return new object?[] { parser, "0.0.0+00", new ParseResult(true, "", 0, 0, 0, null, new[] { "00" }) };
            yield return new object?[] { parser, "0.0.0+00a", new ParseResult(true, "", 0, 0, 0, null, new[] { "00a" }) };
            yield return new object?[] { parser, "0.0.0+0.a", new ParseResult(true, "", 0, 0, 0, null, new[] { "0", "a" }) };
        }

        private static IEnumerable<object?[]> TryParseShouldNotParseStrictFailedGenerator(SemanticVersionParser parser)
        {
            yield return new object?[] { parser, null, new ParseResult(false) };
            yield return new object?[] { parser, "", new ParseResult(false, "") };
            yield return new object?[] { parser, "0.0", new ParseResult(false, "") };
            yield return new object?[] { parser, "00.0.0", new ParseResult(false, "") };
            yield return new object?[] { parser, "0.00.0", new ParseResult(false, "", 0) };
            yield return new object?[] { parser, "0.0.00", new ParseResult(false, "", 0, 0) };
            yield return new object?[] { parser, "01.0.0", new ParseResult(false, "") };
            yield return new object?[] { parser, "0a.0.0", new ParseResult(false, "0a") };
            yield return new object?[] { parser, "0.0a.0", new ParseResult(false, "", 0) };
            yield return new object?[] { parser, "0.0.0a", new ParseResult(false, "", 0, 0) };

            yield return new object?[] { parser, "v0", new ParseResult(false, "v") };
            yield return new object?[] { parser, "v0.0", new ParseResult(false, "v") };
            yield return new object?[] { parser, "V0.0.0", new ParseResult(false, "V") };

            yield return new object?[] { parser, "0.0.0-", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { parser, "0.0.0- ", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { parser, "0.0.0-ä", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { parser, "0.0.0-00", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { parser, "0.0.0-.", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { parser, "0.0.0-a.", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { parser, "0.0.0-.a", new ParseResult(false, "", 0, 0, 0) };

            yield return new object?[] { parser, "0.0.0+", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { parser, "0.0.0+ ", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { parser, "0.0.0+ä", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { parser, "0.0.0+00", new ParseResult(true, "", 0, 0, 0, null, new[] { "00" }) };
            yield return new object?[] { parser, "0.0.0+.", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { parser, "0.0.0+a.", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { parser, "0.0.0+.a", new ParseResult(false, "", 0, 0, 0) };
        }

        private static IEnumerable<object?[]> TryParseShouldParseStrictGenerator()
        {
            var parser = new SemanticVersionParser();

            foreach (var item in TryParseShouldParseStrictSucceededGenerator(parser)) {
                yield return item;
            }

            foreach (var item in TryParseShouldNotParseStrictFailedGenerator(parser)) {
                yield return item;
            }
        }

        private static IEnumerable<object?[]> TryParseShouldParseEraseGenerator()
        {
            var parser = SemanticVersionParser.Erase;

            foreach (var item in TryParseShouldParseStrictSucceededGenerator(parser)) {
                yield return item;
            }

            yield return new object?[] { parser, "00.0.0", new ParseResult(true, "", 0, 0, 0) };
            yield return new object?[] { parser, "0.00.0", new ParseResult(true, "", 0, 0, 0) };
            yield return new object?[] { parser, "0.0.00", new ParseResult(true, "", 0, 0, 0) };

            yield return new object?[] { parser, "v0.0.0", new ParseResult(true, "v", 0, 0, 0) };
            yield return new object?[] { parser, "a0.0.0", new ParseResult(false, "a") };
            yield return new object?[] { parser, "0.a0.0", new ParseResult(false, "", 0) };
            yield return new object?[] { parser, "0.0.a0", new ParseResult(false, "", 0, 0) };

            yield return new object?[] { parser, "\00.0.0", new ParseResult(false, "\0") };

            yield return new object?[] { parser, "0.0.0-00", new ParseResult(true, "", 0, 0, 0, new[] { "0" }) };
            yield return new object?[] { parser, "0.0.0-\00", new ParseResult(true, "", 0, 0, 0, new[] { "0" }) };
            yield return new object?[] { parser, "0.0.0-\0a\00", new ParseResult(true, "", 0, 0, 0, new[] { "a0" }) };

            yield return new object?[] { parser, "0.0.0+00", new ParseResult(true, "", 0, 0, 0, null, new[] { "00" }) };
            yield return new object?[] { parser, "0.0.0+\00", new ParseResult(true, "", 0, 0, 0, null, new[] { "0" }) };
            yield return new object?[] { parser, "0.0.0+\0a\00", new ParseResult(true, "", 0, 0, 0, null, new[] { "a0" }) };
        }

        [Theory]
        [MemberData(nameof(TryParseShouldParseStrictGenerator))]
        [MemberData(nameof(TryParseShouldParseEraseGenerator))]
        public void TryParseShouldParse(SemanticVersionParser parser, string value, ParseResult assumedResult)
        {
            var parseResult = parser.TryParse(value, out var prefix, out var major, out var minor, out var patch, out var preReleaseIdentifiers, out var build);
            Assert.Equal(assumedResult.Result, parseResult);
            Assert.Equal(assumedResult.Prefix, prefix);
            Assert.Equal(assumedResult.Major, major);
            Assert.Equal(assumedResult.Minor, minor);
            Assert.Equal(assumedResult.Patch, patch);
            Assert.Equal(assumedResult.PreReleaseIdentifiers, preReleaseIdentifiers, StringComparer.Ordinal);
            Assert.Equal(assumedResult.Build, build, StringComparer.Ordinal);
        }

        public record ParseResult
        {
            public bool Result { get; }
            public readonly uint? Major, Minor, Patch;
            public readonly IEnumerable<string>? PreReleaseIdentifiers, Build;
            public readonly string? Prefix;

            public ParseResult(bool result, string? prefix, uint? major, uint? minor, uint? patch, IEnumerable<string>? preReleaseIdentifiers, IEnumerable<string>? build)
            {
                Result = result;
                Prefix = prefix;
                Major = major;
                Minor = minor;
                Patch = patch;
                PreReleaseIdentifiers = preReleaseIdentifiers;
                Build = build;
            }

            public ParseResult(bool result, string? prefix, uint? major, uint? minor, uint? patch, IEnumerable<string>? preReleaseIdentifiers)
            {
                Result = result;
                Prefix = prefix;
                Major = major;
                Minor = minor;
                Patch = patch;
                PreReleaseIdentifiers = preReleaseIdentifiers;
            }

            public ParseResult(bool result, string? prefix, uint? major, uint? minor, uint? patch)
            {
                Result = result;
                Prefix = prefix;
                Major = major;
                Minor = minor;
                Patch = patch;
            }

            public ParseResult(bool result, string? prefix, uint? major, uint? minor)
            {
                Result = result;
                Prefix = prefix;
                Major = major;
                Minor = minor;
            }

            public ParseResult(bool result, string? prefix, uint? major)
            {
                Result = result;
                Prefix = prefix;
                Major = major;
            }

            public ParseResult(bool result, string? prefix)
            {
                Result = result;
                Prefix = prefix;
            }

            public ParseResult(bool result)
            {
                Result = result;
            }
        }
    }
}
