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

            services.TryAddScoped<IBranchCasesProvider, BranchCasesProvider>();
            return extensions;
        }

        /// <summary>
        /// Adds multiple branch cases.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="branchCaseArgumentsList"></param>
        public static IGitFeatures AddBranchCases(this IGitFeatures features, IEnumerable<IBranchCase> branchCaseArgumentsList)
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
        /// <param name="branchCases"></param>
        /// <param name="additionalBranchCases"></param>
        public static IGitFeatures AddBranchCases(
            this IGitFeatures features,
            IBranchCase branchCases,
            IEnumerable<IBranchCase> additionalBranchCases)
        {
            return features.AddBranchCases(GetBranchCases(branchCases, additionalBranchCases));

            static IEnumerable<IBranchCase> GetBranchCases(IBranchCase branchCase, IEnumerable<IBranchCase> additionalBranchCases)
            {
                yield return branchCase;

                foreach (var caseArguments in additionalBranchCases) {
                    yield return caseArguments;
                }
            }
        }

        /// <summary>
        /// Applies settings of active branch case by calling:
        /// <br/><see cref="ISinceCommitConfigurer.SetVersionFindingSinceCommit(string?)"/>
        /// <br/><see cref="ISinceCommitConfigurer.SetMessageReadingSinceCommit(string?)"/>
        /// <br/><see cref="IBranchNameConfigurer.SetVersionFindingSinceCommit(string?)"/>
        /// <br/><see cref="IBranchNameConfigurer.SetMessageReadingSinceCommit(string?)"/>
        /// <br/><see cref="IPreReleaseConfigurer.SetSearchPreRelease(string?)"/>:
        /// Either <see cref="IBranchCase.SearchPreRelease"/> or pre-release as explained below is taken.
        /// <br/><see cref="IPreReleaseConfigurer.SetPostPreRelease(string?)"/>:
        /// If <see cref="IBranchCase.PreRelease"/> has no value, so is null and therefore is not "" is specified the value
        /// of <see cref="IBranchCase.Branch"/> or the active branch is taken.
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
