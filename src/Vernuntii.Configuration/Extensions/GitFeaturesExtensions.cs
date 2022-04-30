using Microsoft.Extensions.Configuration;
using Vernuntii.Extensions.BranchCases;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IGitFeatures"/>
    /// </summary>
    public static class GitFeaturesExtensions
    {
        private const string BranchesSectionKey = "Branches";

        /// <summary>
        /// Uses <paramref name="configuration"/> through <paramref name="features"/>:
        /// <br/>- adds repository
        /// <br/>- adds branch cases
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configuration"></param>
        /// <param name="configureBranchCase"></param>
        public static IGitFeatures UseConfigurationDefaults(
            this IGitFeatures features,
            IConfiguration configuration,
            Action<IBranchCase>? configureBranchCase = null) => features
                .AddRepository(options => configuration.Bind(options))
                .AddBranchCases(configuration, configuration.GetSection(BranchesSectionKey).GetChildren(), configureBranchCase);
    }
}
