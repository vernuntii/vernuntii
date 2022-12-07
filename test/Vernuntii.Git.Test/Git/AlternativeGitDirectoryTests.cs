using Xunit;

namespace Vernuntii.Git
{
    public class AlternativeGitDirectoryTests
    {
        [Fact]
        public void GetGitDirectoryShouldResolveAlternativeGitDirectory()
        {
            TemporaryRepositoryOptions repositoryOptions = new();
            using var repository = new TemporaryRepository(repositoryOptions).Resolve();

            var randomFilePath = Path.GetTempFileName();
            var randomFileName = Path.GetFileName(randomFilePath);
            var randomFileDirectory = Path.GetDirectoryName(randomFilePath) ?? throw new InvalidOperationException();

            var directoryContainingDotGit = repositoryOptions.RepositoryOptions.GitWorkingTreeDirectory;
            var dotGitDirectory = Path.Combine(directoryContainingDotGit, ".git");

            File.WriteAllLines(randomFilePath, new string[] { dotGitDirectory });

            var resolvedDirectoryContainingGitDirectory = new AlternativeGitDirectoryResolver(
                Path.Combine(randomFileDirectory, randomFileName),
                DefaultGitDirectoryResolver.Default).ResolveWorkingTreeDirectory("");

            Assert.Equal(directoryContainingDotGit, resolvedDirectoryContainingGitDirectory);
        }
    }
}
