using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii.VersionTransformers
{
    public class PreReleaseTransformerTest
    {
        [Fact]
        public void TransformVersionShouldHavePreRelease()
        {
            var expectedPreRelease = "alpha";

            SemanticVersion expected = SemanticVersion.Zero.With
                .Version(2, 2, 3)
                .PreRelease(expectedPreRelease);

            SemanticVersion versionToTransform = SemanticVersion.Zero.With
                .Version(2, 2, 3)
                .PreRelease("beta");

            PreReleaseTransformer transformer = new(expectedPreRelease);
            var actual = transformer.TransformVersion(versionToTransform);
            Assert.Equal(expected, actual);
        }
    }
}
