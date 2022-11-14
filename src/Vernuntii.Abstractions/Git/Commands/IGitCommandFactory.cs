namespace Vernuntii.Git.Commands
{
    /// <summary>
    /// The factory for an instance of <see cref="IGitCommand"/>.
    /// </summary>
    public interface IGitCommandFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="IGitCommand"/>.
        /// </summary>
        /// <param name="gitDirectory"></param>
        IGitCommand CreateCommand(string gitDirectory);
    }
}
