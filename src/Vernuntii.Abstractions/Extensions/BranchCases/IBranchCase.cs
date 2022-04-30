using Vernuntii.Text.RegularExpressions;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Represents the arguments of a branch case.
    /// </summary>
    public interface IBranchCase
    {
        /// <summary>
        /// Extension interface for various additional features.
        /// </summary>
        IDictionary<string, object> Extensions { get; }

        /// <summary>
        /// The arguments in this instance are used if active branch is equals to this value.
        /// If no instance has a matching <see cref="IfBranch"/> than the default arguments
        /// are used where <see cref="IfBranch"/> is null or empty.
        /// </summary>
        string? IfBranch { get; }


        /// <summary>
        /// The branch reading the commits from.
        /// </summary>
        string? Branch { get; }

        /// <summary>
        /// The since-commit where to start reading from.
        /// </summary>
        string? SinceCommit { get; }

        /// <summary>
        /// The pre-release is used for pre-search or post-transformation.
        /// Used only in pre-search if <see cref="SearchPreRelease"/> is
        /// null.
        /// </summary>
        string? PreRelease { get; }

        /// <summary>
        /// Pre-release escapes.
        /// </summary>
        IReadOnlyCollection<IRegexEscape>? PreReleaseEscapes { get; }

        /// <summary>
        /// The pre-release is used for pre-search.
        /// If null or empty non-pre-release versions are included in
        /// search.
        /// If specified then all non-release AND version with this
        /// pre-release are included in search.
        /// </summary>
        string? SearchPreRelease { get; }

        /// <summary>
        /// Search pre-release escapes.
        /// </summary>
        IReadOnlyCollection<IRegexEscape>? SearchPreReleaseEscapes { get; }
    }
}
