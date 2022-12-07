using FluentAssertions;
using Xunit;

namespace Vernuntii.SemVer.Parser
{
    public class SemanticVersionParserTest
    {
        private static IEnumerable<object?[]> TryParseShouldParseStrictSucceededGenerator(SemanticVersionParser parser)
        {
            yield return new object?[] { parser, "1.1.1", new ParseResult(true, "", 1, 1, 1) };

            yield return new object?[] { parser, "v1.1.1", new ParseResult(true, "v", 1, 1, 1) };

            yield return new object?[] { parser, "1.1.1-a", new ParseResult(true, "", 1, 1, 1, new[] { "a" }) };
            yield return new object?[] { parser, "1.1.1-a.b", new ParseResult(true, "", 1, 1, 1, new[] { "a", "b" }) };
            yield return new object?[] { parser, "1.1.1-01a", new ParseResult(true, "", 1, 1, 1, new[] { "01a" }) };
            yield return new object?[] { parser, "1.1.1-0.a", new ParseResult(true, "", 1, 1, 1, new[] { "0", "a" }) };

            yield return new object?[] { parser, "1.1.1+a", new ParseResult(true, "", 1, 1, 1, null, new[] { "a" }) };
            yield return new object?[] { parser, "1.1.1+a.b", new ParseResult(true, "", 1, 1, 1, null, new[] { "a", "b" }) };
            yield return new object?[] { parser, "1.1.1+01", new ParseResult(true, "", 1, 1, 1, null, new[] { "01" }) };
            yield return new object?[] { parser, "1.1.1+01a", new ParseResult(true, "", 1, 1, 1, null, new[] { "01a" }) };
            yield return new object?[] { parser, "1.1.1+1.a", new ParseResult(true, "", 1, 1, 1, null, new[] { "1", "a" }) };
        }

        private static IEnumerable<object?[]> TryParseShouldNotParseStrictFailedGenerator(SemanticVersionParser parser)
        {
            yield return new object?[] { parser, null, new ParseResult(false) };
            yield return new object?[] { parser, "", new ParseResult(false, "") };
            yield return new object?[] { parser, "0", new ParseResult(false, "") };
            yield return new object?[] { parser, "1", new ParseResult(false, "") };
            yield return new object?[] { parser, "0.0", new ParseResult(false, "") };
            yield return new object?[] { parser, "1.1", new ParseResult(false, "") };
            yield return new object?[] { parser, "00.1.1", new ParseResult(false, "") };
            yield return new object?[] { parser, "01.1.1", new ParseResult(false, "") };
            yield return new object?[] { parser, "1.00.1", new ParseResult(false, "", 1) };
            yield return new object?[] { parser, "1.01.1", new ParseResult(false, "", 1) };
            yield return new object?[] { parser, "1.1.00", new ParseResult(false, "", 1, 1) };
            yield return new object?[] { parser, "1.1.01", new ParseResult(false, "", 1, 1) };
            yield return new object?[] { parser, "01.1.1", new ParseResult(false, "") };
            yield return new object?[] { parser, "1a.1.1", new ParseResult(false, "1a") };
            yield return new object?[] { parser, "1.1a.1", new ParseResult(false, "", 1) };
            yield return new object?[] { parser, "1.1.1a", new ParseResult(false, "", 1, 1) };

            yield return new object?[] { parser, "v1", new ParseResult(false, "v") };
            yield return new object?[] { parser, "v1.1", new ParseResult(false, "v") };
            yield return new object?[] { parser, "V1.1.1", new ParseResult(false, "V") };

            yield return new object?[] { parser, "1.1.1-", new ParseResult(false, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.1.1- ", new ParseResult(false, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.1.1-ä", new ParseResult(false, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.1.1-01", new ParseResult(false, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.1.1-.", new ParseResult(false, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.1.1-a.", new ParseResult(false, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.1.1-.a", new ParseResult(false, "", 1, 1, 1) };

            yield return new object?[] { parser, "1.1.1+", new ParseResult(false, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.1.1+ ", new ParseResult(false, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.1.1+ä", new ParseResult(false, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.1.1+01", new ParseResult(true, "", 1, 1, 1, null, new[] { "01" }) };
            yield return new object?[] { parser, "1.1.1+.", new ParseResult(false, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.1.1+a.", new ParseResult(false, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.1.1+.a", new ParseResult(false, "", 1, 1, 1) };
        }

        public static IEnumerable<object?[]> TryParseShouldParseStrictGenerator()
        {
            SemanticVersionParser parser = new();

            foreach (var item in TryParseShouldParseStrictSucceededGenerator(parser)) {
                yield return item;
            }

            foreach (var item in TryParseShouldNotParseStrictFailedGenerator(parser)) {
                yield return item;
            }
        }

        public static IEnumerable<object?[]> TryParseShouldParseEraseGenerator()
        {
            var parser = SemanticVersionParser.Erase;

            foreach (var item in TryParseShouldParseStrictSucceededGenerator(parser)) {
                yield return item;
            }

            yield return new object?[] { parser, "00.1.1", new ParseResult(true, "", 0, 1, 1) };
            yield return new object?[] { parser, "000.1.1", new ParseResult(true, "", 0, 1, 1) };
            yield return new object?[] { parser, "1.00.1", new ParseResult(true, "", 1, 0, 1) };
            yield return new object?[] { parser, "1.000.1", new ParseResult(true, "", 1, 0, 1) };
            yield return new object?[] { parser, "1.1.00", new ParseResult(true, "", 1, 1, 0) };
            yield return new object?[] { parser, "1.1.000", new ParseResult(true, "", 1, 1, 0) };

            yield return new object?[] { parser, "01.1.1", new ParseResult(true, "", 1, 1, 1) };
            yield return new object?[] { parser, "001.1.1", new ParseResult(true, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.01.1", new ParseResult(true, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.001.1", new ParseResult(true, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.1.01", new ParseResult(true, "", 1, 1, 1) };
            yield return new object?[] { parser, "1.1.001", new ParseResult(true, "", 1, 1, 1) };

            yield return new object?[] { parser, "v0.1.1", new ParseResult(true, "v", 0, 1, 1) };
            yield return new object?[] { parser, "a0.1.1", new ParseResult(false, "a") };
            yield return new object?[] { parser, "0.a0.0", new ParseResult(false, "", 0) };
            yield return new object?[] { parser, "1.1.a0", new ParseResult(false, "", 1, 1) };

            yield return new object?[] { parser, "\01.1.1", new ParseResult(false, "\0") };

            yield return new object?[] { parser, "1.1.1-01", new ParseResult(true, "", 1, 1, 1, new[] { "1" }) };
            yield return new object?[] { parser, "1.1.1-\01", new ParseResult(true, "", 1, 1, 1, new[] { "1" }) };
            yield return new object?[] { parser, "1.1.1-\0a\01", new ParseResult(true, "", 1, 1, 1, new[] { "a1" }) };

            yield return new object?[] { parser, "1.1.1+01", new ParseResult(true, "", 1, 1, 1, null, new[] { "01" }) };
            yield return new object?[] { parser, "1.1.1+\01", new ParseResult(true, "", 1, 1, 1, null, new[] { "1" }) };
            yield return new object?[] { parser, "1.1.1+\0a\01", new ParseResult(true, "", 1, 1, 1, null, new[] { "a1" }) };
        }

        [Theory]
        [MemberData(nameof(TryParseShouldParseStrictGenerator))]
        [MemberData(nameof(TryParseShouldParseEraseGenerator))]
        public void TryParseShouldParse(SemanticVersionParser parser, string value, ParseResult assumedResult)
        {
            var parseResult = parser.TryParse(value, out var prefix, out var major, out var minor, out var patch, out var preReleaseIdentifiers, out var build);
            parseResult.Should().Be(assumedResult.Result);
            prefix.Should().BeEquivalentTo(assumedResult.Prefix);
            major.Should().Be(assumedResult.Major);
            minor.Should().Be(assumedResult.Minor);
            patch.Should().Be(assumedResult.Patch);
            preReleaseIdentifiers.Should().Equal(assumedResult.PreReleaseIdentifiers);
            build.Should().Equal(assumedResult.Build);
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
