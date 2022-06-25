using Vernuntii.VersionPresentation;

namespace Vernuntii.Console
{
    internal class VersionPresentation : IVersionPresentation
    {
        public uint? Major { get; set; }
        public uint? Minor { get; set; }
        public uint? Patch { get; set; }
        public string? Version { get; set; }
        public string? PreRelease { get; set; }
        public string? HyphenPreRelease { get; set; }
        public string? Build { get; set; }
        public string? PlusBuild { get; set; }
        public string? SemanticVersion { get; set; }
        public string? BranchName { get; set; }
        public string? CommitSha { get; set; }
    }
}
