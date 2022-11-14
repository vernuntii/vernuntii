using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii.VersionTransformers
{
    public class NextPatchVersionTransformerTest
    {
        [Fact]
        public void TransformVersionShouldCalculateNextPatch()
        {
            SemanticVersion expected = SemanticVersion.Zero.With.Version(2, 2, 3).ToVersion();
            ISemanticVersion actual = new NextPatchVersionTransformer().TransformVersion(SemanticVersion.Zero.With.Version(2, 2, 2));
            Assert.Equal(expected, actual);
        }
    }
}
