using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii.Git
{
    public class CommitVersionFinderTests
    {
        public static IEnumerable<object[]> FindCommitVersionShouldFindLatestVersionGenerator()
        {
            yield return new[] { "0.0.1", "0.1.0" };
            yield return new[] { "0.1.0", "0.0.1" };
        }

        [Theory]
        [MemberData(nameof(FindCommitVersionShouldFindLatestVersionGenerator))]
        public void FindCommitVersionShouldFindLatestVersion(string firstTag, string secondTag)
        {
            using TemporaryRepository repository = new();
            repository.CommitEmpty();
            repository.TagLightweight(firstTag);
            repository.TagLightweight(secondTag);

            IPositonalCommitVersion? version = new CommitVersionFinder(new CommitVersionFinderOptions(), repository, repository, DefaultCommitVersionFinderLogger)
                .FindCommitVersion(new CommitVersionFindingOptions());

            Assert.NotNull(version);
            Assert.Equal(SemanticVersion.OneMinor, version!, SemanticVersionComparer.VersionReleaseBuild);
        }
    }
}
