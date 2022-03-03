using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii.VersionTransformers
{
    public class NextMinorVersionTransformerTest
    {
        [Fact]
        public void TransformVersionShouldCalculateNextMinor()
        {
            var expected = SemanticVersion.Zero.With.Version(2, 3, 0);
            var actual = new NextMinorVersionTransformer().TransformVersion(SemanticVersion.Zero.With.Version(2, 2, 1));
            Assert.Equal(expected, actual);
        }
    }
}
