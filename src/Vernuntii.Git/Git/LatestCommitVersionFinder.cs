using Microsoft.Extensions.Logging;
using Vernuntii.SemVer;

namespace Vernuntii.Git
{
    /// <summary>
    /// Has capabilities to find the latest version in commit log.
    /// </summary>
    public class LatestCommitVersionFinder : ICommitVersionFinder
    {
        private readonly CommitVersionFinderOptions _options;
        private readonly ICommitVersionsAccessor _commitVersionsAccessor;
        private readonly ICommitsAccessor _commitsAccessor;
        private readonly ILogger<LatestCommitVersionFinder> _logger;
        private readonly Action<ILogger, string, string, string, Exception?> _logLatestVersionSearch;

        /// <summary>
        /// Creates instance of <see cref="LatestCommitVersionFinder"/>.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="commitVersionsAccessor"></param>
        /// <param name="commitsAccessor"></param>
        /// <param name="logger"></param>
        public LatestCommitVersionFinder(
            CommitVersionFinderOptions options,
            ICommitVersionsAccessor commitVersionsAccessor,
            ICommitsAccessor commitsAccessor,
            ILogger<LatestCommitVersionFinder> logger)
        {
            _logLatestVersionSearch = LoggerMessage.Define<string, string, string>(
               LogLevel.Information,
               new EventId(1, "InitialVersion"),
               "Search latest version (Branch = {BranchName}, Since-commit = {SinceCommit}, Search pre-release = {SearchPreRelease})");

            _options = options;
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

        private Dictionary<string, ISemanticVersion> GetVersionTags()
        {
            var versionsByCommitSha = new Dictionary<string, List<ISemanticVersion>>();

            foreach (var commitVersion in _commitVersionsAccessor.GetCommitVersions()) {
                var commitSha = commitVersion.CommitSha;

                if (!versionsByCommitSha.TryGetValue(commitSha, out var versions)) {
                    versions = new List<ISemanticVersion>();
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
        /// <param name="findOptions"></param>
        /// <returns><inheritdoc/></returns>
        public IPositonalCommitVersion? FindCommitVersion(ICommitVersionFindOptions findOptions)
        {
            var branchName = findOptions.BranchName;
            var sinceCommit = findOptions.SinceCommit;
            var searchingPreRelease = findOptions.PreRelease;
            LogLatestVersionSearch(branchName, sinceCommit, searchingPreRelease);

            var versionByCommitDictionary = GetVersionTags();
            var currentCommitGap = 0;

            foreach (var commit in _commitsAccessor.GetCommits(
                branchName: findOptions.BranchName,
                sinceCommit: findOptions.SinceCommit,
                fromOldToNew: false)) {
                if (versionByCommitDictionary.TryGetValue(commit.Sha, out var version)) {
                    if (_options.PreReleaseMatcher is null) {
                        _logger.LogWarning("A pre-release matcher to find the latest commit version was not found, so fallback has matched the first appearing commit version: {Version}", version);
                    } else if (!_options.PreReleaseMatcher.IsMatch(
                        searchingPreRelease: searchingPreRelease,
                        facingPreRelease: version.PreRelease)) {
                        continue;
                    }

                    //var isVersionReleaseEmpty = string.IsNullOrEmpty(version.PreRelease);

                    //if (!isVersionReleaseEmpty) {
                    //    if (string.IsNullOrEmpty(preReleaseToFind)) {
                    //        continue;
                    //    }

                    //    if (!string.IsNullOrEmpty(preReleaseToFind) && !string.Equals(preReleaseToFind, version.PreRelease, StringComparison.OrdinalIgnoreCase)) {
                    //        continue;
                    //    }
                    //}

                    return new PositionalCommitVersion(commit.Sha, currentCommitGap++, version);
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if version core of <paramref name="version"/> already exists.
        /// </summary>
        /// <param name="version"></param>
        /// <returns>True if version core is already released.</returns>
        public bool IsVersionReleased(ISemanticVersion version) =>
            _commitVersionsAccessor.HasCommitVersion(version);
    }
}
