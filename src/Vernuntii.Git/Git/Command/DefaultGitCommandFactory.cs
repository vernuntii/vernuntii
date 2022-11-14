namespace Vernuntii.Git.Commands
{
    /// <summary>
    /// The default git command factory.
    /// </summary>
    public class DefaultGitCommandFactory : IGitCommandFactory
    {
        /// <summary>
        /// The default instance of this type.
        /// </summary>
        public static readonly DefaultGitCommandFactory Default = new();

        /// <inheritdoc/>
        public IGitCommand CreateCommand(string gitDirectory) =>
            new GitCommand(gitDirectory);
    }
}
