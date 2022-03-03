using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Vernuntii.Extensions.Configurers;
using Vernuntii.Text.RegularExpressions;
using Teronis;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IGitFeatures"/>.
    /// </summary>
    public static class GitFeaturesExtensions
    {
        private static IGitFeatures AddBranchCaseArgumentsProvider(this IGitFeatures extensions)
        {
            var services = extensions.Services;

            services.TryAddScoped(sp =>
                new SlimLazy<IOptionsSnapshot<BranchCasesOptions>>(
                    sp.GetRequiredService<IOptionsSnapshot<BranchCasesOptions>>));

            services.TryAddScoped<IBranchCaseArgumentsProvider, BranchCaseArgumentsProvider>();
            return extensions;
        }

        /// <summary>
        /// Adds multiple branch cases.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="branchCaseArgumentsList"></param>
        public static IGitFeatures AddBranchCases(this IGitFeatures features, IEnumerable<IBranchCaseArguments> branchCaseArgumentsList)
        {
            features.AddBranchCaseArgumentsProvider();

            var services = features.Services;
            services.ConfigureOptions<BranchCasesOptions.PostConfiguration>();

            services.AddOptions<BranchCasesOptions>()
                .Configure(options => {
                    foreach (var caseArguments in branchCaseArgumentsList) {
                        options.AddBranchCase(caseArguments);
                    }
                });

            return features;
        }

        /// <summary>
        /// Adds multiple branch cases.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="branchCaseArguments"></param>
        /// <param name="additionalBranchCaseArguments"></param>
        public static IGitFeatures AddBranchCases(
            this IGitFeatures features,
            IBranchCaseArguments branchCaseArguments,
            IEnumerable<IBranchCaseArguments> additionalBranchCaseArguments)
        {
            return features.AddBranchCases(GetBranchCases(branchCaseArguments, additionalBranchCaseArguments));

            static IEnumerable<IBranchCaseArguments> GetBranchCases(IBranchCaseArguments branchCaseArguments, IEnumerable<IBranchCaseArguments> additionalBranchCaseArguments)
            {
                yield return branchCaseArguments;

                foreach (var caseArguments in additionalBranchCaseArguments) {
                    yield return caseArguments;
                }
            }
        }

        /// <summary>
        /// Configures instances of <see cref="IBranchCaseArguments"/> of <see cref="BranchCasesOptions"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configureBranchCase"></param>
        public static IGitFeatures ConfigureBranchCases(this IGitFeatures features, Action<IBranchCaseArguments> configureBranchCase)
        {
            features.Services.AddOptions<BranchCasesOptions>()
                .Configure(options => {
                    foreach (var branchCaseArguments in options.BranchCases.Values) {
                        configureBranchCase(branchCaseArguments);
                    }
                });

            return features;
        }

        /// <summary>
        /// Applies settings of active branch case by calling:
        /// <br/><see cref="ISinceCommitConfigurer.SetVersionFindingSinceCommit(string?)"/>
        /// <br/><see cref="ISinceCommitConfigurer.SetMessageReadingSinceCommit(string?)"/>
        /// <br/><see cref="IBranchNameConfigurer.SetVersionFindingSinceCommit(string?)"/>
        /// <br/><see cref="IBranchNameConfigurer.SetMessageReadingSinceCommit(string?)"/>
        /// <br/><see cref="IPreReleaseConfigurer.SetSearchPreRelease(string?)"/>:
        /// Either <see cref="IBranchCaseArguments.SearchPreRelease"/> or pre-release as explained below is taken.
        /// <br/><see cref="IPreReleaseConfigurer.SetPostPreRelease(string?)"/>:
        /// If <see cref="IBranchCaseArguments.PreRelease"/> has no value, so is null and therefore is not "" is specified the value
        /// of <see cref="IBranchCaseArguments.Branch"/> or the active branch is taken.
        /// If "" (default) then no pre-release is taken. The non-empty pre-release that is taken by the one or the other way is used
        /// to search "&lt;major>.&lt;minor>.&lt;patch>"- and "&lt;major>.&lt;minor>.&lt;patch>-&lt;taken-pre-release>"-versions
        /// otherwise only "&lt;major>.&lt;minor>.&lt;patch>"-versions are considered.
        /// </summary>
        /// <param name="features"></param>
        public static IGitFeatures UseActiveBranchCaseDefaults(this IGitFeatures features) => features
            .ConfigureSinceCommit(configurer => {
                var branchCaseArguments = configurer.ServiceProvider.GetActiveBranchCase();
                configurer.SetVersionFindingSinceCommit(branchCaseArguments.SinceCommit);
                configurer.SetMessageReadingSinceCommit(branchCaseArguments.SinceCommit);
            })
            .ConfigureBranchName(configurer => {
                var branchCaseArguments = configurer.ServiceProvider.GetActiveBranchCase();
                configurer.SetVersionFindingSinceCommit(branchCaseArguments.Branch);
                configurer.SetMessageReadingSinceCommit(branchCaseArguments.Branch);
            })
            .ConfigurePreRelease(configurer => {
                var defaultBranchCaseArguments = configurer.ServiceProvider.GetDefaultBranchCase();
                var activeBranchCaseArguments = configurer.ServiceProvider.GetActiveBranchCase();

                // Define pre-release.
                var preRelease = activeBranchCaseArguments.PreRelease;

                if (preRelease == null) {
                    preRelease = activeBranchCaseArguments.Branch ?? configurer.Repository.GetActiveBranch().ShortBranchName;
                } else if (preRelease == "") {
                    preRelease = null;
                }

                // Define search pre-release.
                string? searchPreRelease = activeBranchCaseArguments.SearchPreRelease ?? preRelease;

                // Escape search pre-release.
                searchPreRelease = RegexUtils.Escape(searchPreRelease, activeBranchCaseArguments.SearchPreReleaseEscapes
                    ?? activeBranchCaseArguments.PreReleaseEscapes
                    ?? defaultBranchCaseArguments.SearchPreReleaseEscapes
                    ?? defaultBranchCaseArguments.PreReleaseEscapes);

                // Escape pre-release.
                preRelease = RegexUtils.Escape(preRelease, activeBranchCaseArguments.PreReleaseEscapes
                    ?? defaultBranchCaseArguments.PreReleaseEscapes);

                configurer.SetSearchPreRelease(searchPreRelease);
                configurer.SetPostPreRelease(preRelease);
            });
    }
}
