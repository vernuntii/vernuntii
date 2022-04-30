using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii.VersionTransformers
{
    public class NextMajorVersionTransformerTest
    {
        [Fact]
        public void TransformVersionShouldCalculateNextMajor()
        {
            var expected = SemanticVersion.Zero.With.Major(3).ToVersion();
            var actual = new NextMajorVersionTransformer().TransformVersion(SemanticVersion.Zero.With.Version(2, 1, 1));
            Assert.Equal(expected, actual);
        }
    }
}
