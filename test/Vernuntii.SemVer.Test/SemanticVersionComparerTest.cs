using Vernuntii.Collections;
using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii
{
    public class SemanticVersionComparerTest
    {
        [Fact]
        public void Release_versions_should_be_sorted_ascended()
        {
            var expectedVersions = new[] {
                SemanticVersion.Parse("0.0.0"),
                SemanticVersion.Parse("0.0.1")
            };

            var actualVersions = expectedVersions.Order(SemanticVersionComparer.VersionReleaseBuild).ToList();
            Assert.Equal(expectedVersions, actualVersions, SemanticVersionComparer.VersionReleaseBuild);
        }

        [Fact]
        public void Pre_releases_should_be_sorted_ascended()
        {
            var expectedVersions = new[] {
                SemanticVersion.Parse("0.0.0-alpha"),
                SemanticVersion.Parse("0.0.0-beta")
            };

            var actualVersions = expectedVersions.Order(SemanticVersionComparer.VersionReleaseBuild).ToList();
            Assert.Equal(expectedVersions, actualVersions, SemanticVersionComparer.VersionReleaseBuild);
        }

        [Fact]
        public void Pre_release_with_build_precedes_equivalent_pre_release()
        {
            var expectedVersions = new[] {
                SemanticVersion.Parse("0.0.0-alpha"),
                SemanticVersion.Parse("0.0.0-alpha+1"),
            };

            var actualVersions = expectedVersions.Order(SemanticVersionComparer.VersionReleaseBuild).ToList();
            Assert.Equal(expectedVersions, actualVersions, SemanticVersionComparer.VersionReleaseBuild);
        }

        [Fact]
        public void Pre_releases_should_be_sorted_ordinal_insensitive()
        {
            /* See https://github.com/semver/semver/pull/276 */

            var expectedVersions = new[] {
                SemanticVersion.Parse("0.0.1-alpha"),
                SemanticVersion.Parse("0.0.1-America")
            };

            var actualVersions = expectedVersions.Order(SemanticVersionComparer.VersionReleaseBuild).ToList();
            Assert.Equal(expectedVersions, actualVersions, SemanticVersionComparer.VersionReleaseBuild);
        }

        [Fact]
        public void Pre_release_should_come_prior_release_version()
        {
            var expectedVersions = new[] {
                SemanticVersion.Parse("0.0.0-alpha"),
                SemanticVersion.Parse("0.0.0")
            };

            var actualVersions = expectedVersions.Order(SemanticVersionComparer.VersionReleaseBuild).ToList();
            Assert.Equal(expectedVersions, actualVersions, SemanticVersionComparer.VersionReleaseBuild);
        }

        [Fact]
        public void Build_should_precede_release_version()
        {
            var expectedVersions = new[] {
                SemanticVersion.Parse("0.0.0"),
                SemanticVersion.Parse("0.0.0+1")
            };

            var actualVersions = expectedVersions.Order(SemanticVersionComparer.VersionReleaseBuild).ToList();
            Assert.Equal(expectedVersions, actualVersions, SemanticVersionComparer.VersionReleaseBuild);
        }
    }
}
