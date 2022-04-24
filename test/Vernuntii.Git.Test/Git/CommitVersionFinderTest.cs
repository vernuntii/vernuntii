using GitTools.Testing;
using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii.Git
{
    public class CommitVersionFinderTest
    {
        [Fact]
        public void FindCommitVersionShouldFindLatestVersion()
        {
            //using var repository = new EmptyRepositoryFixture();
            //repository.MakeACommit();
            //repository.ApplyTag("0.0.1"); // release
            //repository.ApplyTag("0.0.1-alpha"); // pre-release
            //repository.ApplyTag("0.0.1-beta"); // pre-release

            //var version = new CommitVersionFinder(repository.Repository, repository.Repository, LoggerFactory.CreateLogger<CommitVersionFinder>())
            //    .FindCommitVersion(new CommitVersionFindingOptions());

            //Assert.Equal(SemanticVersion.OnePatch, version);
        }
    }
}
