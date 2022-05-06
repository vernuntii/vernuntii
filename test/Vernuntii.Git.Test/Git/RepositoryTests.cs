using Xunit;

namespace Vernuntii.Git
{
    public class RepositoryTests
    {
        [Fact]
        public void RepositoryResolveShouldThrowShallowRepositoryException()
        {
            var logger = LoggerFactory.CreateLogger<TemporaryRepository>();

            var originOptions = new TemporaryRepositoryOptions();
            using var origin = new TemporaryRepository(originOptions, logger);
            origin.CommitEmpty();
            origin.CommitEmpty();

            var shallowOptions = new TemporaryRepositoryOptions() {
                CloneOptions = new CloneOptions(originOptions.RepositoryOptions.GitDirectory) {
                    Source = CloneSource.File,
                    Depth = 1
                },
            };

            using var shallow = new TemporaryRepository(shallowOptions, logger).Resolve();
            _ = Assert.IsType<ShallowRepositoryException>(Record.Exception(() => new Repository(shallowOptions.RepositoryOptions, logger).Resolve()));
        }

        [Fact]
        public void RepositoryResolveShouldNotThrowShallowRepositoryException()
        {
            var logger = LoggerFactory.CreateLogger<TemporaryRepository>();
            var originOptions = new TemporaryRepositoryOptions();
            using var origin = new TemporaryRepository(originOptions, logger).Resolve();

            Assert.Null(Record.Exception(() => new Repository(originOptions.RepositoryOptions, logger).Resolve()));
        }
    }
}
