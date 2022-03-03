using Vernuntii.SemVer;
using Vernuntii.VersionFoundation;

namespace Vernuntii.VersionPresentation
{
    internal sealed record SemanticVersionPresentation : ISemanticVersionPresentation
    {
        public static SemanticVersionPresentation Create(
            ISemanticVersionFoundation presentationFoundation,
            SemanticVersionPresentationPart presentableParts = SemanticVersionPresentationPart.All)
        {
            var versionPresentation = new SemanticVersionPresentation();
            var version = presentationFoundation.Version;

            if (presentableParts.HasFlag(SemanticVersionPresentationPart.Major)) {
                versionPresentation.Major = version.Major;
            }

            if (presentableParts.HasFlag(SemanticVersionPresentationPart.Minor)) {
                versionPresentation.Minor = version.Minor;
            }

            if (presentableParts.HasFlag(SemanticVersionPresentationPart.Patch)) {
                versionPresentation.Patch = version.Patch;
            }

            if (presentableParts.HasFlag(SemanticVersionPresentationPart.Version)) {
                versionPresentation.Version = version.ToString(SemanticVersionFormat.Version);
            }

            if (presentableParts.HasFlag(SemanticVersionPresentationPart.PreRelease)) {
                versionPresentation.PreRelease = version.PreRelease;
            }

            if (presentableParts.HasFlag(SemanticVersionPresentationPart.HyphenPreRelease)) {
                versionPresentation.HyphenPreRelease = version.ToString(SemanticVersionFormat.PreRelease);
            }

            if (presentableParts.HasFlag(SemanticVersionPresentationPart.Build)) {
                versionPresentation.Build = version.Build;
            }

            if (presentableParts.HasFlag(SemanticVersionPresentationPart.PlusBuild)) {
                versionPresentation.PlusBuild = version.ToString(SemanticVersionFormat.Build);
            }

            if (presentableParts.HasFlag(SemanticVersionPresentationPart.SemanticVersion)) {
                versionPresentation.SemanticVersion = version.ToString(SemanticVersionFormat.SemanticVersion);
            }

            if (presentableParts.HasFlag(SemanticVersionPresentationPart.BranchName)) {
                versionPresentation.BranchName = presentationFoundation.BranchName;
            }

            if (presentableParts.HasFlag(SemanticVersionPresentationPart.CommitSha)) {
                versionPresentation.CommitSha = presentationFoundation.CommitSha;
            }

            return versionPresentation;
        }

        private SemanticVersionPresentation()
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

        public object? GetValue(SemanticVersionPresentationPart presentationPart) => presentationPart switch {
            SemanticVersionPresentationPart.Major => Major,
            SemanticVersionPresentationPart.Minor => Minor,
            SemanticVersionPresentationPart.Patch => Patch,
            SemanticVersionPresentationPart.Version => Version,
            SemanticVersionPresentationPart.PreRelease => PreRelease,
            SemanticVersionPresentationPart.HyphenPreRelease => HyphenPreRelease,
            SemanticVersionPresentationPart.Build => Build,
            SemanticVersionPresentationPart.PlusBuild => PlusBuild,
            SemanticVersionPresentationPart.SemanticVersion => SemanticVersion,
            _ => throw new NotSupportedException("Presentation part does not represent a value (Have you specified two parts?)")
        };
    }
}
