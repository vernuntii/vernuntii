using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Vernuntii.Plugins;
using Task = Microsoft.Build.Utilities.Task;

namespace Vernuntii.Console.MSBuild
{
    /// <summary>
    /// <see cref="Vernuntii"/>-specific task that can be called from MSBuild.
    /// </summary>
    public class VernuntiiTask : Task
    {
        /// <summary>
        /// The path to the <see cref="Vernuntii"/>-specific console executable.
        /// </summary>
        [Required]
        public string? ConsoleExecutablePath { get; set; }

        /// <summary>
        /// See console executable help for more informations.
        /// </summary>
        [Required]
        public string? ConfigPath { get; set; }

        /// <summary>
        /// See console executable help for more informations.
        /// </summary>
        public string? Verbosity { get; set; }

        /// <summary>
        /// The cache id is used to cache the version informations once and load them on next accesses.
        /// </summary>
        public string? CacheId { get; set; }

        /// <summary>
        /// The cache retention time since creation.
        /// </summary>
        public string? CacheCreationRetentionTime { get; set; }

        /// <summary>
        /// The cache retention time since last access.
        /// </summary>
        public string? CacheLastAccessRetentionTime { get; set; }

        /// <summary>
        /// Indicates that the caches should be emptied where version informations are stored. This happens before the cache process itself.
        /// </summary>
        public bool EmptyCaches { get; set; }

        /// <summary>
        /// The timeout in seconds at which the daemon will be terminated after no new action. If -1, no timeout is used.
        /// </summary>
        public int DaemonTimeout { get; set; }

        /// <summary>
        /// Major version number.
        /// </summary>
        [Output]
        public string? Major { get; set; }

        /// <summary>
        /// Minor version number.
        /// </summary>
        [Output]
        public string? Minor { get; set; }

        /// <summary>
        /// Patch version number.
        /// </summary>
        [Output]
        public string? Patch { get; set; }

        /// <summary>
        /// E.g. 0.1.0
        /// </summary>
        [Output]
        public string? VersionCore { get; set; }

        /// <summary>
        /// E.g. alpha
        /// </summary>
        [Output]
        public string? PreRelease { get; set; }

        /// <summary>
        /// E.g. -alpha
        /// </summary>
        [Output]
        public string? HyphenPreRelease { get; set; }

        /// <summary>
        /// E.g. 3
        /// </summary>
        [Output]
        public string? Build { get; set; }

        /// <summary>
        /// E.g. +3
        /// </summary>
        [Output]
        public string? PlusBuild { get; set; }

        /// <summary>
        /// E.g. 0.1.0-alpha+3
        /// </summary>
        [Output]
        public string? SemanticVersion { get; set; }

        /// <summary>
        /// The current branch name.
        /// </summary>
        [Output]
        public string? BranchName { get; set; }

        /// <summary>
        /// The commit sha of current branch.
        /// </summary>
        [Output]
        public string? BranchTip { get; set; }

        private async Task<bool> ExecuteAsync() {
            try {
                var executor = new ConsoleProcessExecutor(logger: Log);

                var executionArguments = new ConsoleProcessExecutionArguments() {
                    ConsoleExecutablePath = ConsoleExecutablePath,
                    Verbosity = Verbosity,
                    ConfigPath = ConfigPath,
                    CacheId = CacheId,
                    CacheCreationRetentionTime = CacheCreationRetentionTime,
                    CacheLastAccessRetentionTime = CacheLastAccessRetentionTime,
                    EmptyCaches = EmptyCaches,
                    DaemonTimeout = DaemonTimeout
                };

                var version = await executor.ExecuteAsync(executionArguments);
                Major = version.Major?.ToString(CultureInfo.InvariantCulture);
                Minor = version.Major?.ToString(CultureInfo.InvariantCulture);
                Patch = version.Major?.ToString(CultureInfo.InvariantCulture);
                VersionCore = version.VersionCore;
                PreRelease = version.PreRelease;
                HyphenPreRelease = version.HyphenPreRelease;
                Build = version.Build;
                PlusBuild = version.PlusBuild;
                SemanticVersion = version.SemanticVersion;
                BranchName = version.BranchName;
                BranchTip = version.BranchTip;
            } catch (NextVersionApiException error) {
                Log.LogError(error.ToString());
            } catch (Exception error) {
                Log.LogError($"Error during console process execution: {error}");
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
        public override bool Execute() =>
            ExecuteAsync().GetAwaiter().GetResult();
    }
}
