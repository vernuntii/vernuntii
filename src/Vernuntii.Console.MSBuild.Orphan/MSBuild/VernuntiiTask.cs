using System;
using System.Globalization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

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
        public string? Version { get; set; }

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
        public string? CommitSha { get; set; }

        /// <inheritdoc/>
        public override bool Execute()
        {
            try {
                var executor = new ConsoleProcessExecutor(
                    consoleExecutablePath: ConsoleExecutablePath!,
                    logger: Log);

                var version = executor.Execute(
                    verbosity: Verbosity,
                    configPath: ConfigPath!,
                    cacheId: CacheId,
                    cacheCreationRetentionTime: CacheCreationRetentionTime,
                    cacheLastAccessRetentionTime: CacheLastAccessRetentionTime,
                    emptyCaches: EmptyCaches);

                Major = version.Major?.ToString(CultureInfo.InvariantCulture);
                Minor = version.Major?.ToString(CultureInfo.InvariantCulture);
                Patch = version.Major?.ToString(CultureInfo.InvariantCulture);
                Version = version.Version;
                PreRelease = version.PreRelease;
                HyphenPreRelease = version.HyphenPreRelease;
                Build = version.Build;
                PlusBuild = version.PlusBuild;
                SemanticVersion = version.SemanticVersion;
                BranchName = version.BranchName;
                CommitSha = version.CommitSha;
            } catch (Exception error) {
                Log.LogError(error.Message ?? "An error occurred while executing console process");
                return false;
            }

            return true;
        }
    }
}
