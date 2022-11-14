using Xunit;

namespace Vernuntii.Git
{
    public class AlternativeGitDirectoryTests
    {
        [Fact]
        public void GetGitDirectoryShouldResolveAlternativeGitDirectory()
        {
            TemporaryRepositoryOptions repositoryOptions = new();
            using TemporaryRepository repository = new TemporaryRepository(repositoryOptions).Resolve();

            string randomFilePath = Path.GetTempFileName();
            string randomFileName = Path.GetFileName(randomFilePath);
            string randomFileDirectory = Path.GetDirectoryName(randomFilePath) ?? throw new InvalidOperationException();

            string directoryContainingDotGit = repositoryOptions.RepositoryOptions.GitWorkingTreeDirectory;
            string dotGitDirectory = Path.Combine(directoryContainingDotGit, ".git");

            File.WriteAllLines(randomFilePath, new string[] { dotGitDirectory });

            string resolvedDirectoryContainingGitDirectory = new AlternativeGitDirectoryResolver(
                Path.Combine(randomFileDirectory, randomFileName),
                DefaultGitDirectoryResolver.Default).ResolveWorkingTreeDirectory("");

            Assert.Equal(directoryContainingDotGit, resolvedDirectoryContainingGitDirectory);
        }
    }
}
