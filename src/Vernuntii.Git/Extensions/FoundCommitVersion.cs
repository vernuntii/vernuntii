using Microsoft.Extensions.Options;
using Vernuntii.Git;
using Vernuntii.SemVer;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Caches the result of <see cref="ICommitVersionFinder"/> once requested.
    /// </summary>
    internal class FoundCommitVersion
    {
        /// <summary>
        /// The found commit version.
        /// </summary>
        public IPositonalCommitVersion? CommitVersion {
            get {
                if (_searchedForCommitVersion) {
                    return _commitVersion;
                }

                _searchedForCommitVersion = true;
                return _commitVersion = _commitVersionFinder.FindCommitVersion(_options.Value);
            }
        }

        /// <summary>
        /// True means that the version core of <see cref="CommitVersion"/>
        /// has not been already used.
        /// </summary>
        public bool IsCommitVersionCoreAlreadyReleased {
            get {
                if (_searchedForIsCommitVersionCoreAlreadyReleased) {
                    return _isCommitVersionCoreAlreadyReleased;
                }

                _searchedForIsCommitVersionCoreAlreadyReleased = true;
                var commitVersion = CommitVersion;
                var options = _options.Value;

                if (commitVersion != null) {
                    var isPostVersionPreRelease = !options.IsPostVersionPreRelease.HasValue
                        ? commitVersion.IsPreRelease
                        : options.IsPostVersionPreRelease.Value;

                    ISemanticVersion versionCore = commitVersion;
                    var isCommitVersionCoreAlreadyReleased = false;

                    if (isPostVersionPreRelease) {
                        versionCore = commitVersion.With().PreRelease(options.PostPreRelease).WithoutBuild().ToVersion();
                        isCommitVersionCoreAlreadyReleased = _commitVersionsAccessor.HasCommitVersion(versionCore);
                    }

                    _isCommitVersionCoreAlreadyReleased = isCommitVersionCoreAlreadyReleased
                        || _commitVersionsAccessor.HasCommitVersion(versionCore.With().WithoutPreRelease().ToVersion());
                } else {
                    _isCommitVersionCoreAlreadyReleased = false;
                }

                return _isCommitVersionCoreAlreadyReleased;
            }
        }

        private readonly ICommitVersionFinder _commitVersionFinder;
        private readonly ICommitVersionsAccessor _commitVersionsAccessor;
        private readonly IOptionsSnapshot<FoundCommitVersionOptions> _options;

        private bool _searchedForCommitVersion;
        private IPositonalCommitVersion? _commitVersion;

        private bool _searchedForIsCommitVersionCoreAlreadyReleased;
        private bool _isCommitVersionCoreAlreadyReleased;

        public FoundCommitVersion(
            ICommitVersionFinder commitVersionFinder,
            ICommitVersionsAccessor commitVersionsAccessor,
            IOptionsSnapshot<FoundCommitVersionOptions> options)
        {
            _commitVersionFinder = commitVersionFinder;
            _commitVersionsAccessor = commitVersionsAccessor;
            _options = options;
        }
    }
}
