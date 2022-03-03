using Xunit;

namespace Vernuntii.SemVer.Parser
{
    public class SemanticVersionParserTest
    {
        private static IEnumerable<object?[]> TryParseShouldParseStrictGenerator()
        {
            yield return new object?[] { null, new ParseResult(false) };
            yield return new object?[] { "", new ParseResult(false, "") };
            yield return new object?[] { "0.0", new ParseResult(false, "") };
            yield return new object?[] { "0.0.0", new ParseResult(true, "", 0, 0, 0) };
            yield return new object?[] { "00.0.0", new ParseResult(false, "") };
            yield return new object?[] { "0.00.0", new ParseResult(false, "", 0) };
            yield return new object?[] { "0.0.00", new ParseResult(false, "", 0, 0) };
            yield return new object?[] { "01.0.0", new ParseResult(false, "") };
            yield return new object?[] { "0a.0.0", new ParseResult(false, "0a") };
            yield return new object?[] { "0.0a.0", new ParseResult(false, "", 0) };
            yield return new object?[] { "0.0.0a", new ParseResult(false, "", 0, 0) };

            yield return new object?[] { "v0", new ParseResult(false, "v") };
            yield return new object?[] { "v0.0", new ParseResult(false, "v") };
            yield return new object?[] { "v0.0.0", new ParseResult(true, "v", 0, 0, 0) };
            yield return new object?[] { "V0.0.0", new ParseResult(false, "V") };

            yield return new object?[] { "0.0.0-", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { "0.0.0- ", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { "0.0.0-ä", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { "0.0.0-a", new ParseResult(true, "", 0, 0, 0, new[] { "a" }) };
            yield return new object?[] { "0.0.0-a.b", new ParseResult(true, "", 0, 0, 0, new[] { "a", "b" }) };
            yield return new object?[] { "0.0.0-00", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { "0.0.0-00a", new ParseResult(true, "", 0, 0, 0, new[] { "00a" }) };
            yield return new object?[] { "0.0.0-0.a", new ParseResult(true, "", 0, 0, 0, new[] { "0", "a" }) };
            yield return new object?[] { "0.0.0-.", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { "0.0.0-a.", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { "0.0.0-.a", new ParseResult(false, "", 0, 0, 0) };

            yield return new object?[] { "0.0.0+", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { "0.0.0+ ", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { "0.0.0+ä", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { "0.0.0+a", new ParseResult(true, "", 0, 0, 0, null, new[] { "a" }) };
            yield return new object?[] { "0.0.0+a.b", new ParseResult(true, "", 0, 0, 0, null, new[] { "a", "b" }) };
            yield return new object?[] { "0.0.0+00", new ParseResult(true, "", 0, 0, 0, null, new[] { "00" }) };
            yield return new object?[] { "0.0.0+00a", new ParseResult(true, "", 0, 0, 0, null, new[] { "00a" }) };
            yield return new object?[] { "0.0.0+0.a", new ParseResult(true, "", 0, 0, 0, null, new[] { "0", "a" }) };
            yield return new object?[] { "0.0.0+.", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { "0.0.0+a.", new ParseResult(false, "", 0, 0, 0) };
            yield return new object?[] { "0.0.0+.a", new ParseResult(false, "", 0, 0, 0) };
        }

        [Theory]
        [MemberData(nameof(TryParseShouldParseStrictGenerator))]
        public void TryParseShouldParseStrict(string value, ParseResult assumedResult)
        {
            var versionSeparater = new SemanticVersionParser();
            var parseResult = versionSeparater.TryParse(value, out var prefix, out var major, out var minor, out var patch, out var preReleaseIdentifiers, out var build);
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
