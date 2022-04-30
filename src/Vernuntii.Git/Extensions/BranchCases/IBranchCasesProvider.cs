namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Provides instances of <see cref="IBranchCase"/>.
    /// </summary>
    public interface IBranchCasesProvider
    {
        /// <summary>
        /// The branch cases with default branch case.
        /// </summary>
        IReadOnlyDictionary<string, IBranchCase> BranchCases { get; }

        /// <summary>
        /// The nested branch cases without default branch case.
        /// </summary>
        IReadOnlyDictionary<string, IBranchCase> NestedBranchCases { get; }

        /// <summary>
        /// The case arguments for active branch.
        /// </summary>
        IBranchCase ActiveBranchCase { get; }

        /// <summary>
        /// The case arguments for default branch.
        /// </summary>
        IBranchCase DefaultBranchCase { get; }
    }
}
