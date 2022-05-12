namespace Vernuntii.Git
{
    /// <summary>
    /// Matcher to find pre-release candidate.
    /// </summary>
    public interface ICommitVersionPreReleaseMatcher
    {
        /// <summary>
        /// Checks whether <paramref name="preReleaseCandiate"/> is candidate.
        /// </summary>
        /// <param name="preReleaseToFind"></param>
        /// <param name="preReleaseCandiate"></param>
        /// <returns>True means match.</returns>
        bool IsMatch(string? preReleaseToFind, string preReleaseCandiate);
    }
}
