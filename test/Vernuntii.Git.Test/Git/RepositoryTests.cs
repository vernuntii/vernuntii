using Xunit;

namespace Vernuntii.Git
{
    public class RepositoryTests
    {
        [Fact]
        public void RepositoryResolveShouldThrowShallowRepositoryException()
        {
            var originOptions = new TemporaryRepositoryOptions();
            using var origin = new TemporaryRepository(originOptions, DefaultTemporaryRepositoryLogger);
            origin.CommitEmpty();
            origin.CommitEmpty();

            var shallowOptions = new TemporaryRepositoryOptions() {
                CloneOptions = new CloneOptions(originOptions.RepositoryOptions.GitDirectory) {
                    Source = CloneSource.File,
                    Depth = 1
                },
            };

            using var shallow = new TemporaryRepository(shallowOptions, DefaultTemporaryRepositoryLogger).Resolve();
            _ = Assert.IsType<ShallowRepositoryException>(Record.Exception(() => new Repository(shallowOptions.RepositoryOptions, DefaultTemporaryRepositoryLogger).Resolve()));
        }

        [Fact]
        public void RepositoryResolveShouldNotThrowShallowRepositoryException()
        {
            var originOptions = new TemporaryRepositoryOptions();
            using var origin = new TemporaryRepository(originOptions, DefaultTemporaryRepositoryLogger).Resolve();

            Assert.Null(Record.Exception(() => new Repository(originOptions.RepositoryOptions, DefaultTemporaryRepositoryLogger).Resolve()));
        }
    }
}
