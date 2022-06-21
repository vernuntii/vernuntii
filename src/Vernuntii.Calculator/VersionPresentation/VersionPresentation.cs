using Vernuntii.SemVer;
using Vernuntii.VersionCaching;

namespace Vernuntii.VersionPresentation
{
    internal sealed record VersionPresentation : IVersionPresentation
    {
        public static VersionPresentation Create(
            IVersionCache presentationCache,
            VersionPresentationPart presentableParts = VersionPresentationPart.All)
        {
            var versionPresentation = new VersionPresentation();
            var version = presentationCache.Version;

            if (presentableParts.HasFlag(VersionPresentationPart.Major)) {
                versionPresentation.Major = version.Major;
            }

            if (presentableParts.HasFlag(VersionPresentationPart.Minor)) {
                versionPresentation.Minor = version.Minor;
            }

            if (presentableParts.HasFlag(VersionPresentationPart.Patch)) {
                versionPresentation.Patch = version.Patch;
            }

            if (presentableParts.HasFlag(VersionPresentationPart.Version)) {
                versionPresentation.Version = version.Format(SemanticVersionFormat.Version);
            }

            if (presentableParts.HasFlag(VersionPresentationPart.PreRelease)) {
                versionPresentation.PreRelease = version.PreRelease;
            }

            if (presentableParts.HasFlag(VersionPresentationPart.HyphenPreRelease)) {
                versionPresentation.HyphenPreRelease = version.Format(SemanticVersionFormat.PreRelease);
            }

            if (presentableParts.HasFlag(VersionPresentationPart.Build)) {
                versionPresentation.Build = version.Build;
            }

            if (presentableParts.HasFlag(VersionPresentationPart.PlusBuild)) {
                versionPresentation.PlusBuild = version.Format(SemanticVersionFormat.Build);
            }

            if (presentableParts.HasFlag(VersionPresentationPart.SemanticVersion)) {
                versionPresentation.SemanticVersion = version.Format(SemanticVersionFormat.SemanticVersion);
            }

            if (presentableParts.HasFlag(VersionPresentationPart.BranchName)) {
                versionPresentation.BranchName = presentationCache.BranchName;
            }

            if (presentableParts.HasFlag(VersionPresentationPart.CommitSha)) {
                versionPresentation.CommitSha = presentationCache.BranchTip;
            }

            return versionPresentation;
        }

        private VersionPresentation()
        {
        }

        public uint? Major { get; set; }
        public uint? Minor { get; set; }
        public uint? Patch { get; set; }
        public string? Version { get; set; }
        public string? PreRelease { get; set; }
        public string? HyphenPreRelease { get; set; }
        public string? Build { get; set; }
        public string? PlusBuild { get; set; }
        public string? PreReleaseBuild { get; set; }
        public string? SemanticVersion { get; set; }
        public string? BranchName { get; set; }
        public string? CommitSha { get; set; }

        public object? GetValue(VersionPresentationPart presentationPart) => presentationPart switch {
            VersionPresentationPart.Major => Major,
            VersionPresentationPart.Minor => Minor,
            VersionPresentationPart.Patch => Patch,
            VersionPresentationPart.Version => Version,
            VersionPresentationPart.PreRelease => PreRelease,
            VersionPresentationPart.HyphenPreRelease => HyphenPreRelease,
            VersionPresentationPart.Build => Build,
            VersionPresentationPart.PlusBuild => PlusBuild,
            VersionPresentationPart.SemanticVersion => SemanticVersion,
            _ => throw new NotSupportedException("Presentation part does not represent a value (Have you specified two parts?)")
        };
    }
}
