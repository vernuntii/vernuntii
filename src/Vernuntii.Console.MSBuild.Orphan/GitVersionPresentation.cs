namespace Vernuntii.Console
{
    internal class GitVersionPresentation
    {
        /// <summary>
        /// Major version number.
        /// </summary>
        public uint? Major { get; set; }

        /// <summary>
        /// Minor version number.
        /// </summary>
        public uint? Minor { get; set; }


        /// <summary>
        /// Patch version number.
        /// </summary>
        public uint? Patch { get; set; }

        /// <summary>
        /// E.g. 0.1.0
        /// </summary>
        public string? VersionCore { get; set; }

        /// <summary>
        /// E.g. alpha
        /// </summary>
        public string? PreRelease { get; set; }

        /// <summary>
        /// E.g. -alpha
        /// </summary>
        public string? HyphenPreRelease { get; set; }

        /// <summary>
        /// E.g. 3
        /// </summary>
        public string? Build { get; set; }

        /// <summary>
        /// E.g. +3
        /// </summary>
        public string? PlusBuild { get; set; }

        /// <summary>
        /// 0.1.0-alpha+3
        /// </summary>
        public string? SemanticVersion { get; set; }

        /// <summary>
        /// The current branch name.
        /// </summary>
        public string? BranchName { get; set; }

        /// <summary>
        /// The commit sha of the current branch.
        /// </summary>
        public string? CommitSha { get; set; }
    }
}
