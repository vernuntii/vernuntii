namespace Vernuntii.Git;

/// <summary>
/// The find options used in <see cref="ICommitVersionFinder.FindCommitVersion(ICommitVersionFindOptions)"/>.
/// </summary>
public interface ICommitVersionFindOptions
{
    /// <summary>
    /// The since-commit where to start reading from.
    /// </summary>
    string? BranchName { get; }

    /// <summary>
    /// The branch reading the commits from.
    /// </summary>
    string? PreRelease { get; }

    /// <summary>
    /// If null or empty non-pre-release versions are included in
    /// search.
    /// If specified then all non-release AND version with this
    /// pre-release are included in search.
    /// </summary>
    string? SinceCommit { get; }
}
