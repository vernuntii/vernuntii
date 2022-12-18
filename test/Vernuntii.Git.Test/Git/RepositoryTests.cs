using FluentAssertions;
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

            var shallowOptions = new TemporaryRepositoryOptions() {
                CloneOptions = new CloneOptions(originOptions.CommandOptions.GitWorkingTreeDirectory) {
                    Source = CloneSource.File,
                    Depth = 1
                },
            };

            var createShallowOptions = () => new TemporaryRepository(shallowOptions, DefaultTemporaryRepositoryLogger);
            _ = FluentActions.Invoking(createShallowOptions).Should().ThrowExactly<ShallowRepositoryException>();
        }

        [Fact]
        public void RepositoryResolveShouldNotThrowShallowRepositoryException()
        {
            TemporaryRepositoryOptions originOptions = new();
            using var origin = new TemporaryRepository(originOptions, DefaultTemporaryRepositoryLogger);

            Assert.Null(Record.Exception(() => Repository.Create(originOptions, DefaultTemporaryRepositoryLogger)));
        }
    }
}
