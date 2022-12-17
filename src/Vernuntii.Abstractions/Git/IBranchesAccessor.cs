namespace Vernuntii.Git;

public interface IBranchesAccessor
{
    /// <summary>
    /// A collection of branches.
    /// </summary>
    IBranches Branches { get; }

    /// <summary>
    /// Gets active branch.
    /// </summary>
    IBranch GetActiveBranch();

    /// <summary>
    /// Expands branch name to long name.
    /// </summary>
    /// <param name="branchName">Short or partial branch name.</param>
    /// <returns>Long branch name or <see langword="null"/>.</returns>
    string? ExpandBranchName(string? branchName);
}
