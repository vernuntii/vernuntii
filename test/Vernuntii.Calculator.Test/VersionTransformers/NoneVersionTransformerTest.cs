using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii.VersionTransformers
{
    public class NoneVersionTransformerTest
    {
        [Fact]
        public void TransformVersionShouldNotCalculateNextVersion()
        {
            SemanticVersion expected = SemanticVersion.Zero.With.Version(2, 2, 2).ToVersion();
            ISemanticVersion actual = new NoneVersionTransformer().TransformVersion(SemanticVersion.Zero.With.Version(2, 2, 2));
            Assert.Equal(expected, actual);
        }
    }
}
