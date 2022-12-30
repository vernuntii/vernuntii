using Vernuntii.Configuration;
using Vernuntii.Configuration.Queueing;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.Git.Commands;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IFileDerivedConfigurationProviderQueue"/>.
    /// </summary>
    public static class FileDerivedConfigurationProviderQueueExtensions
    {
        /// <summary>
        /// Shadows keys in following cases:
        /// <br/>1. If key <see cref="GitCommandOptions.GitWorkingTreeDirectory"/> is not set, then it will be shadowed by directory containing the configuration file.
        /// If key <see cref="GitCommandOptions.GitWorkingTreeDirectory"/> is set but relative, then it will be shadowed by its path relative to the directory containing
        /// the configuration file.
        /// <br/>2. If PreRelease- or SearchPreRelease-key is existing but null, then it will be shadowed by a collection with one null entry. The null entry
        /// won't be parsable, so results into an empty collection what the desired effect is, otherwise the key is ignored and treated as non-existing from
        /// Microsoft.Extensions.Configuration.Binder, or in other words: the user wouldn't be able to "reset" a PreRelease- or SearchPreRelease-key because
        /// a default could be implemented in case of an actual non-existing PreRelease- or SearchPreRelease-key, so the user could never "reset" the default.
        /// </summary>
        /// <param name="configurator"></param>
        /// <exception cref="ArgumentException"></exception>
        public static IFileDerivedConfigurationProviderQueue AddGitDefaults(this IFileDerivedConfigurationProviderQueue configurator)
        {
            //var fileContainingDirectory = configurator.FileInfo.Directory ?? throw new ArgumentException("File is not in an existing directory");
            //configurator.AddShadow(new GitDirectoryCorrectionConfigurationProvider(fileContainingDirectory, configurator.RootConfigurationProvider));

            configurator.Enqueue(new EmptyArrayCorrectionConfigurationProvider(configurator.RootConfigurationProvider) {
                $"/{nameof(IBranchCase.PreReleaseEscapes)}$/",
                $"/{nameof(IBranchCase.SearchPreReleaseEscapes)}$/",
            });

            return configurator;
        }
    }
}
