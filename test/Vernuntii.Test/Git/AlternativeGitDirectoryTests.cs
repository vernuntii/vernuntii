namespace Vernuntii.Git
{
    public class AlternativeGitDirectoryTests
    {
        [Fact]
        public void GetGitDirectoryShouldResolveAlternativeGitDirectory()
        {
            TemporaryRepositoryOptions repositoryOptions = new();
            using var repository = new TemporaryRepository(repositoryOptions);

            var randomFilePath = Path.GetTempFileName();
            var randomFileName = Path.GetFileName(randomFilePath);
            var randomFileDirectory = Path.GetDirectoryName(randomFilePath) ?? throw new InvalidOperationException();

            var directoryContainingDotGit = repositoryOptions.CommandOptions.GitWorkingTreeDirectory;
            var dotGitDirectory = Path.Combine(directoryContainingDotGit, ".git");

            File.WriteAllLines(randomFilePath, new string[] { dotGitDirectory });

            var resolvedDirectoryContainingGitDirectory = new GitDirectoryResolver() {
                VernuntiiGitFilename = Path.Combine(randomFileDirectory, randomFileName)
            }.ResolveWorkingTreeDirectory(repositoryOptions.CommandOptions.GitWorkingTreeDirectory);

            Assert.Equal(directoryContainingDotGit, resolvedDirectoryContainingGitDirectory);
        }
    }
}
