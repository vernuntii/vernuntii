using Xunit;

namespace Vernuntii.Git
{
    public class RepositoryBranchesTests
    {
        [Fact]
        public void BranchesShouldContainHead()
        {
            using var repository = new TemporaryRepository(DefaultTemporaryRepositoryLogger);
            repository.CommitEmpty();
            repository.CommitEmpty();
            repository.Checkout("HEAD~");
            Assert.NotNull(repository.Branches["HEAD"]);
        }

        [Fact]
        public void BranchesShouldNotContainHead()
        {
            using var repository = new TemporaryRepository(DefaultTemporaryRepositoryLogger);
            repository.CommitEmpty();
            Assert.Null(repository.Branches["HEAD"]);
        }
    }
}
