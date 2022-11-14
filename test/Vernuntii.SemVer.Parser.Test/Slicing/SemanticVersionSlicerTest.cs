using Xunit;

namespace Vernuntii.SemVer.Parser.Slicing
{
    public class SemanticVersionSlicerTest
    {
        public static IEnumerable<object?[]> TrySliceShouldSliceGenerator()
        {
            yield return new object?[] { null, new SliceResult(false) };
            yield return new object?[] { "", new SliceResult(false, "", "") };
            yield return new object?[] { "a", new SliceResult(false, "a", "") };
            yield return new object?[] { "a0", new SliceResult(false, "a", "0") };
            yield return new object?[] { "0", new SliceResult(false, "", "0") };
            yield return new object?[] { ".", new SliceResult(false, "", "", "") };
            yield return new object?[] { "..", new SliceResult(true, "", "", "", "") };
            yield return new object?[] { "..+", new SliceResult(true, "", "", "", "", null, "") };
            yield return new object?[] { "..++", new SliceResult(true, "", "", "", "", null, "+") };
            yield return new object?[] { "..+-", new SliceResult(true, "", "", "", "", null, "-") };
            yield return new object?[] { "..-", new SliceResult(true, "", "", "", "", "", null) };
            yield return new object?[] { "..-+", new SliceResult(true, "", "", "", "", "", "") };
            yield return new object?[] { "..-+-", new SliceResult(true, "", "", "", "", "", "-") };
            yield return new object?[] { "..--+", new SliceResult(true, "", "", "", "", "-", "") };
        }

        [Theory]
        [MemberData(nameof(TrySliceShouldSliceGenerator))]
        public void TrySliceShouldSlice(string value, SliceResult assumedResult)
        {
            SemanticVersionSlicer versionSeparater = SemanticVersionSlicer.Default;
            Assert.Equal(assumedResult.Result, versionSeparater.TrySlice(value, out string? prefix, out string? major, out string? minor, out string? patch, out string? preRelease, out string? build));
            Assert.Equal(assumedResult.Prefix, prefix);
            Assert.Equal(assumedResult.Major, major);
            Assert.Equal(assumedResult.Minor, minor);
            Assert.Equal(assumedResult.Patch, patch);
            Assert.Equal(assumedResult.PreRelease, preRelease);
            Assert.Equal(assumedResult.Build, build);
        }

        public record SliceResult
        {
            public bool Result { get; }
            public readonly string? Prefix, Major, Minor, Patch, PreRelease, Build;

            public SliceResult(bool result, string? prefix, string? major, string? minor, string? patch, string? preRelease, string? build)
            {
                Result = result;
                Prefix = prefix;
                Major = major;
                Minor = minor;
                Patch = patch;
                PreRelease = preRelease;
                Build = build;
            }

            public SliceResult(bool result, string? prefix, string? major, string? minor, string? patch, string? preRelease)
            {
                Result = result;
                Prefix = prefix;
                Major = major;
                Minor = minor;
                Patch = patch;
                PreRelease = preRelease;
            }

            public SliceResult(bool result, string? prefix, string? major, string? minor, string? patch)
            {
                Result = result;
                Prefix = prefix;
                Major = major;
                Minor = minor;
                Patch = patch;
            }

            public SliceResult(bool result, string? prefix, string? major, string? minor)
            {
                Result = result;
                Prefix = prefix;
                Major = major;
                Minor = minor;
            }

            public SliceResult(bool result, string? prefix, string? major)
            {
                Result = result;
                Prefix = prefix;
                Major = major;
            }

            public SliceResult(bool result, string? prefix)
            {
                Result = result;
                Prefix = prefix;
            }

            public SliceResult(bool result)
            {
                Result = result;
            }
        }
    }
}
