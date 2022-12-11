using System.Diagnostics.CodeAnalysis;
using Vernuntii.Git;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// The options for <see cref="ICommitVersionFinder.FindCommitVersion(ICommitVersionFindOptions)"/>
    /// </summary>
    internal sealed class FoundCommitVersionOptions : ICommitVersionFindOptions
    {
        /// <inheritdoc/>
        public string? SinceCommit { get; set; }

        /// <inheritdoc/>
        public string? BranchName { get; set; }

        /// <inheritdoc/>
        public string? SearchPreRelease { get; set; }

        string? ICommitVersionFindOptions.PreRelease => SearchPreRelease;

        [MemberNotNullWhen(true, nameof(PostPreRelease))]
        public bool? IsPostVersionPreRelease { get; set; }

        public string? PostPreRelease { get; set; }
    }
}
