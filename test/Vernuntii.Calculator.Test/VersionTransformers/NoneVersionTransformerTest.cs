using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii.VersionTransformers
{
    public class NoneVersionTransformerTest
    {
        [Fact]
        public void TransformVersionShouldNotCalculateNextVersion()
        {
            var expected = SemanticVersion.Zero.With.Version(2, 2, 2).ToVersion();
            var actual = new NoneVersionTransformer().TransformVersion(SemanticVersion.Zero.With.Version(2, 2, 2));
            Assert.Equal(expected, actual);
        }
    }
}
