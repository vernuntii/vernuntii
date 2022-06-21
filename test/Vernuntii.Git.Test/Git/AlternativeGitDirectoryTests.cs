using Xunit;

namespace Vernuntii.Git
{
    public class AlternativeGitDirectoryTests
    {
        [Fact]
        public void GetGitDirectoryShouldResolveAlternativeGitDirectory()
        {
            var repositoryOptions = new TemporaryRepositoryOptions();
            using var repository = new TemporaryRepository(repositoryOptions).Resolve();

            var randomFilePath = Path.GetTempFileName();
            var randomFileName = Path.GetFileName(randomFilePath);
            var randomFileDirectory = Path.GetDirectoryName(randomFilePath) ?? throw new InvalidOperationException();

            var directoryContainingDotGit = repositoryOptions.RepositoryOptions.GitDirectory;
            var dotGitDirectory = Path.Combine(directoryContainingDotGit, ".git");

            File.WriteAllLines(randomFilePath, new string[] { dotGitDirectory });

            var resolvedDirectoryContainingGitDirectory = new AlternativeGitDirectoryResolver(
                Path.Combine(randomFileDirectory, randomFileName),
                DefaultGitDirectoryResolver.Default).ResolveGitDirectory("");

            Assert.Equal(directoryContainingDotGit, resolvedDirectoryContainingGitDirectory);
        }
    }
}
