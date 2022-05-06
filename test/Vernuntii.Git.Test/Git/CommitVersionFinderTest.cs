using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii.Git
{
    public class CommitVersionFinderTest
    {
        [Fact]
        public void FindCommitVersionShouldFindLatestVersion()
        {
            using var repository = new TemporaryRepository(LoggerFactory.CreateLogger<TemporaryRepository>());
            repository.CommitEmpty();
            repository.TagLightweight("0.0.1");
            repository.TagLightweight("0.1.0");

            var version = new CommitVersionFinder(new CommitVersionFinderOptions(), repository, repository, LoggerFactory.CreateLogger<CommitVersionFinder>())
                .FindCommitVersion(new CommitVersionFindingOptions());

            Assert.NotNull(version);
            Assert.Equal(SemanticVersion.OneMinor, version!, SemanticVersionComparer.VersionReleaseBuild);
        }
    }
}
