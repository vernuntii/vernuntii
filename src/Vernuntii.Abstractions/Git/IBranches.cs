namespace Vernuntii.Git
{
    /// <summary>
    /// A collection of branches.
    /// </summary>
    public interface IBranches : IReadOnlyList<IBranch>
    {
        /// <summary>
        /// Gets branch by name.
        /// </summary>
        /// <param name="branchName"></param>
        /// <returns></returns>
        IBranch? this[string branchName] { get; }
    }
}
