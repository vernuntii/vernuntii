using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii.VersionTransformers
{
    public class NextMajorVersionTransformerTest
    {
        [Fact]
        public void TransformVersionShouldCalculateNextMajor()
        {
            SemanticVersion expected = SemanticVersion.Zero.With.Major(3).ToVersion();
            ISemanticVersion actual = new NextMajorVersionTransformer().TransformVersion(SemanticVersion.Zero.With.Version(2, 1, 1));
            Assert.Equal(expected, actual);
        }
    }
}
