using Microsoft.Extensions.Logging;

namespace Vernuntii.Git
{
    /// <summary>
    /// Has capabilities to find a version in commit log.
    /// </summary>
    public class CommitVersionFinder : ICommitVersionFinder
    {
        private readonly ICommitVersionsAccessor _commitVersionsAccessor;
        private readonly ICommitsAccessor _commitsAccessor;
        private readonly ILogger<CommitVersionFinder> _logger;
        private readonly Action<ILogger, string, string, string, Exception?> _logLatestVersionSearch;

        /// <summary>
        /// Creates instance of <see cref="CommitVersionFinder"/>.
        /// </summary>
        /// <param name="commitVersionsAccessor"></param>
        /// <param name="commitsAccessor"></param>
        /// <param name="logger"></param>
        public CommitVersionFinder(
            ICommitVersionsAccessor commitVersionsAccessor,
            ICommitsAccessor commitsAccessor,
            ILogger<CommitVersionFinder> logger)
        {
            _logLatestVersionSearch = LoggerMessage.Define<string, string, string>(
               LogLevel.Information,
               new EventId(1, "InitialVersion"),
               "Search latest version (Branch = {BranchName}, Since-commit = {SinceCommit}, Search pre-release = {SearchPreRelease})");

            _commitVersionsAccessor = commitVersionsAccessor ?? throw new ArgumentNullException(nameof(commitVersionsAccessor));
            _commitsAccessor = commitsAccessor ?? throw new ArgumentNullException(nameof(commitsAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private static string GetNonEmptyQuotedStringOrDefault(string? value, string defaultValue) => string.IsNullOrEmpty(value)
            ? defaultValue
            : $"\"{value}\"";

        private void LogLatestVersionSearch(string? branchName, string? sinceCommit, string? searchPreReleaseIdentifier) =>
            _logLatestVersionSearch(
                _logger,
                GetNonEmptyQuotedStringOrDefault(branchName, "Active branch"),
                GetNonEmptyQuotedStringOrDefault(sinceCommit, "Not specified"),
                GetNonEmptyQuotedStringOrDefault(searchPreReleaseIdentifier, "only releases"),
                null);

        private Dictionary<string, SemanticVersion> GetVersionTags()
        {
            var versionsByCommitSha = new Dictionary<string, List<SemanticVersion>>();

            foreach (var commitVersion in _commitVersionsAccessor.GetCommitVersions()) {
                var commitSha = commitVersion.CommitSha;

                if (!versionsByCommitSha.TryGetValue(commitSha, out var versions)) {
                    versions = new List<SemanticVersion>();
                    versionsByCommitSha.Add(commitSha, versions);
                }

                versions.Add(commitVersion);
            }

            foreach (var (_, versions) in versionsByCommitSha) {
                versions.Sort(SemanticVersionComparer.VersionReleaseBuild);
            }

            return versionsByCommitSha.ToDictionary(pair => pair.Key, pair => pair.Value.Last());
        }

        /// <summary>
        /// Finds a version of the first appearing
        /// commit that are sorted from newest to oldest.
        /// </summary>
        /// <param name="findingOptions"></param>
        /// <returns><inheritdoc/></returns>
        public CommitVersion? FindCommitVersion(CommitVersionFindingOptions findingOptions)
        {
            var branchName = findingOptions.BranchName;
            var sinceCommit = findingOptions.SinceCommit;
            var preRelease = findingOptions.PreRelease;
            LogLatestVersionSearch(branchName, sinceCommit, preRelease);

            var versionByCommitDictionary = GetVersionTags();

            foreach (var commit in _commitsAccessor.GetCommits(
                branchName: findingOptions.BranchName,
                sinceCommit: findingOptions.SinceCommit,
                reverse: false)) {
                if (versionByCommitDictionary.TryGetValue(commit.Sha, out var version)) {
                    var isVersionReleaseEmpty = string.IsNullOrEmpty(version.PreRelease);

                    if (!isVersionReleaseEmpty) {
                        if (string.IsNullOrEmpty(preRelease)) {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(preRelease) && !string.Equals(preRelease, version.PreRelease, StringComparison.OrdinalIgnoreCase)) {
                            continue;
                        }
                    }

                    return new CommitVersion(version, commit.Sha);
                }
            }

            return null;
        }
    }
}
