namespace Vernuntii.Git
{
    /// <summary>
    /// Matcher to find pre-release candidate.
    /// </summary>
    public interface ICommitVersionPreReleaseMatcher
    {
        /// <summary>
        /// Checks whether <paramref name="facingPreRelease"/> is matching <paramref name="searchingPreRelease"/>.
        /// </summary>
        /// <param name="searchingPreRelease">The pre-release we searching for.</param>
        /// <param name="facingPreRelease">The pre-release we are facing.</param>
        /// <returns><see langword="true"/> means they match.</returns>
        bool IsMatch(string? searchingPreRelease, string facingPreRelease);
    }
}
