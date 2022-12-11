using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Teronis;
using Vernuntii.Git;
using Vernuntii.Text.RegularExpressions;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IGitServicesScope"/>.
    /// </summary>
    public static class GitFeaturesExtensions
    {
        private static IGitServicesScope AddBranchCaseArgumentsProvider(this IGitServicesScope extensions)
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
        public static IGitServicesScope AddBranchCases(this IGitServicesScope features, IEnumerable<IBranchCase> branchCaseArgumentsList)
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
        public static IGitServicesScope AddBranchCases(
            this IGitServicesScope features,
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
        /// <br/><see cref="IGitConfigurer.SetSinceCommit(string?)"/>
        /// <br/><see cref="IGitConfigurer.SetBranch(string?)"/>
        /// <br/><see cref="IGitConfigurer.SetSearchPreRelease(string?)"/>:
        /// Either <see cref="IBranchCase.SearchPreRelease"/> or pre-release as explained below is taken.
        /// <br/><see cref="IGitConfigurer.SetPostPreRelease(string?)"/>:
        /// If <see cref="IBranchCase.PreRelease"/> has no value, so is null and therefore is not "" is specified the value
        /// of <see cref="IBranchCase.Branch"/> or the active branch is taken.
        /// If "" (default) then no pre-release is taken. The non-empty pre-release that is taken by the one or the other way is used
        /// to search "&lt;major>.&lt;minor>.&lt;patch>"- and "&lt;major>.&lt;minor>.&lt;patch>-&lt;taken-pre-release>"-versions
        /// otherwise only "&lt;major>.&lt;minor>.&lt;patch>"-versions are considered.
        /// </summary>
        /// <param name="features"></param>
        public static IGitServicesScope UseActiveBranchCaseDefaults(this IGitServicesScope features) => features
            .Configure(configurer => {
                var defaultBranchCaseArguments = configurer.ServiceProvider.GetDefaultBranchCase();
                var activeBranchCaseArguments = configurer.ServiceProvider.GetActiveBranchCase();
                var repository = configurer.ServiceProvider.GetRequiredService<IRepository>();

                configurer.SetSinceCommit(activeBranchCaseArguments.SinceCommit);
                configurer.SetBranch(activeBranchCaseArguments.Branch);

                // Define pre-release.
                var preRelease = activeBranchCaseArguments.PreRelease;

                if (preRelease == null) {
                    preRelease = activeBranchCaseArguments.Branch ?? repository.GetActiveBranch().ShortBranchName;
                } else if (preRelease == "") {
                    preRelease = null;
                }

                // Define search pre-release.
                var searchPreRelease = activeBranchCaseArguments.SearchPreRelease ?? preRelease;

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
