namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Provides instances of <see cref="IBranchCaseArguments"/>.
    /// </summary>
    public interface IBranchCaseArgumentsProvider
    {
        /// <summary>
        /// The branch cases with default branch case.
        /// </summary>
        IReadOnlyDictionary<string, IBranchCaseArguments> BranchCases { get; }

        /// <summary>
        /// The nested branch cases without default branch case.
        /// </summary>
        IReadOnlyDictionary<string, IBranchCaseArguments> NestedBranchCases { get; }

        /// <summary>
        /// The case arguments for active branch.
        /// </summary>
        IBranchCaseArguments ActiveBranchCase { get; }

        /// <summary>
        /// The case arguments for default branch.
        /// </summary>
        IBranchCaseArguments DefaultBranchCase { get; }
    }
}
