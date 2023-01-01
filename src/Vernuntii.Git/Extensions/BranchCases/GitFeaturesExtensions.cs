using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vernuntii.Git;
using Vernuntii.Text.RegularExpressions;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IGitServicesView"/>.
    /// </summary>
    public static class GitFeaturesExtensions
    {
        private static IGitServicesView AddBranchCaseArgumentsProvider(this IGitServicesView view)
        {
            var services = view.Services;
            services.TryAddScoped<IBranchCasesProvider, BranchCasesProvider>();
            return view;
        }

        /// <summary>
        /// Adds multiple branch cases.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="branchCaseArgumentsList"></param>
        public static IGitServicesView AddBranchCases(this IGitServicesView view, IEnumerable<IBranchCase> branchCaseArgumentsList)
        {
            view.AddBranchCaseArgumentsProvider();

            var services = view.Services;
            services.ConfigureOptions<BranchCasesOptions.PostConfiguration>();

            services.AddOptions<BranchCasesOptions>()
                .Configure(options => {
                    foreach (var caseArguments in branchCaseArgumentsList) {
                        options.AddBranchCase(caseArguments);
                    }
                });

            return view;
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
        /// <param name="view"></param>
        public static IGitServicesView UseActiveBranchCaseDefaults(this IGitServicesView view) => view.Configure(configurer => {
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
