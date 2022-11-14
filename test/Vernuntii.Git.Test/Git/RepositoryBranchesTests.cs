using Xunit;

namespace Vernuntii.Git
{
    public class RepositoryBranchesTests
    {
        [Fact]
        public void BranchesShouldContainHead()
        {
            using TemporaryRepository repository = new();
            repository.CommitEmpty();
            repository.CommitEmpty();
            repository.Checkout("HEAD~");
            Assert.NotNull(repository.Branches["HEAD"]);
        }

        [Fact]
        public void BranchesShouldNotContainHead()
        {
            using TemporaryRepository repository = new();
            repository.CommitEmpty();
            Assert.Null(repository.Branches["HEAD"]);
        }
    }
}
