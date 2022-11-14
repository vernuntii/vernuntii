using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii
{
    public class SemanticVersionComparerTest
    {
        [Fact]
        public void SortShouldSortVersionsAscended()
        {
            SemanticVersion versionZeroRelease = SemanticVersion.Parse("0.0.0");
            SemanticVersion versionOneRelease = SemanticVersion.Parse("0.0.1");
            SemanticVersion versionOneAlpha = SemanticVersion.Parse("0.0.1-alpha");
            SemanticVersion versionOneAmerica = SemanticVersion.Parse("0.0.1-America");
            SemanticVersion versionOneBeta = SemanticVersion.Parse("0.0.1-beta");
            SemanticVersion versionOneAlphaBuild = SemanticVersion.Parse("0.0.1-alpha+1");
            SemanticVersion versionOneReleaseBuild = SemanticVersion.Parse("0.0.1+1");
            SemanticVersion versionTwoRelease = SemanticVersion.Parse("0.0.2");

            List<SemanticVersion> versions = new() {
                versionOneBeta,
                versionTwoRelease,
                versionOneReleaseBuild,
                versionOneAlphaBuild,
                versionOneRelease,
                versionOneAmerica,
                versionZeroRelease,
                versionOneAlpha,
            };

            versions.Sort(SemanticVersionComparer.VersionReleaseBuild);

            Assert.Equal(
                new[] {
                    versionZeroRelease,
                    versionOneAlpha,
                    versionOneAlphaBuild,
                    versionOneAmerica,
                    versionOneBeta,
                    versionOneRelease,
                    versionOneReleaseBuild,
                    versionTwoRelease
                },
                versions,
                SemanticVersionComparer.VersionReleaseBuild);
        }
    }
}
