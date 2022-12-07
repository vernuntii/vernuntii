using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii
{
    public class SemanticVersionComparerTest
    {
        [Fact]
        public void SortShouldSortVersionsAscended()
        {
            var versionZeroRelease = SemanticVersion.Parse("0.0.0");
            var versionOneRelease = SemanticVersion.Parse("0.0.1");
            var versionOneAlpha = SemanticVersion.Parse("0.0.1-alpha");
            var versionOneAmerica = SemanticVersion.Parse("0.0.1-America");
            var versionOneBeta = SemanticVersion.Parse("0.0.1-beta");
            var versionOneAlphaBuild = SemanticVersion.Parse("0.0.1-alpha+1");
            var versionOneReleaseBuild = SemanticVersion.Parse("0.0.1+1");
            var versionTwoRelease = SemanticVersion.Parse("0.0.2");

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
