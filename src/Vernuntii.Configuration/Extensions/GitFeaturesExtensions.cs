using Microsoft.Extensions.Configuration;
using Vernuntii.Extensions.BranchCases;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IGitServicesScope"/>
    /// </summary>
    public static class GitFeaturesExtensions
    {
        private const string BranchesSectionKey = "Branches";

        /// <summary>
        /// Uses <paramref name="configuration"/> through <paramref name="features"/>:
        /// <br/>- adds branch cases
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configuration"></param>
        /// <param name="configureBranchCase"></param>
        public static IGitServicesScope UseConfigurationDefaults(
            this IGitServicesScope features,
            IConfiguration configuration,
            Action<IBranchCase>? configureBranchCase = null) => features
                .AddBranchCases(configuration, configuration.GetSection(BranchesSectionKey).GetChildren(), configureBranchCase);
    }
}
