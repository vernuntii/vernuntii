using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii.VersionTransformers
{
    public class NextPatchVersionTransformerTest
    {
        [Fact]
        public void TransformVersionShouldCalculateNextPatch()
        {
            var expected = SemanticVersion.Zero.With.Version(2,2,3);
            var actual = new NextPatchVersionTransformer().TransformVersion(SemanticVersion.Zero.With.Version(2,2,2));
            Assert.Equal(expected, actual);
        }
    }
}
