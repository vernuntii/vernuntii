using Xunit;

namespace Vernuntii.Git
{
    public class RepositoryTests
    {
        [Fact]
        public void RepositoryResolveShouldThrowShallowRepositoryException()
        {
            TemporaryRepositoryOptions originOptions = new();
            using TemporaryRepository origin = new(originOptions, DefaultTemporaryRepositoryLogger);
            origin.CommitEmpty();
            origin.CommitEmpty();

            TemporaryRepositoryOptions shallowOptions = new() {
                CloneOptions = new CloneOptions(originOptions.RepositoryOptions.GitWorkingTreeDirectory) {
                    Source = CloneSource.File,
                    Depth = 1
                },
            };

            using TemporaryRepository shallow = new TemporaryRepository(shallowOptions, DefaultTemporaryRepositoryLogger).Resolve();
            _ = Assert.IsType<ShallowRepositoryException>(Record.Exception(() => new Repository(shallowOptions.RepositoryOptions, DefaultTemporaryRepositoryLogger).Resolve()));
        }

        [Fact]
        public void RepositoryResolveShouldNotThrowShallowRepositoryException()
        {
            TemporaryRepositoryOptions originOptions = new();
            using TemporaryRepository origin = new TemporaryRepository(originOptions, DefaultTemporaryRepositoryLogger).Resolve();

            Assert.Null(Record.Exception(() => new Repository(originOptions.RepositoryOptions, DefaultTemporaryRepositoryLogger).Resolve()));
        }
    }
}
