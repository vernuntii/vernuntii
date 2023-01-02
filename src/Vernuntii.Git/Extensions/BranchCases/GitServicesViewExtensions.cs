using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vernuntii.MessageConventions;
using Vernuntii.VersionIncrementFlows;
using Vernuntii.VersioningPresets;
using Vernuntii.Git;
using Vernuntii.Text.RegularExpressions;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IGitServicesView"/>.
    /// </summary>
    public static class GitServicesViewExtensions
    {
        private static BranchCase CreateBranchCase(IConfiguration configuration, Action<IBranchCase>? configureBranchCase)
        {
            var branchCaseArguments = new BranchCase();
            configuration.Bind(branchCaseArguments);
            branchCaseArguments.Extensions[BranchCase.ConfigurationExtensionName] = configuration;
            configureBranchCase?.Invoke(branchCaseArguments);
            return branchCaseArguments;
        }

        private static IGitServicesView AddBranchCasesProvider(this IGitServicesView view)
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
            view.AddBranchCasesProvider();

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
        /// Adds multiple branch cases from configuration sections.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="branchCaseSections"></param>
        /// <param name="configureBranchCase"></param>
        public static IGitServicesView AddBranchCases(
            this IGitServicesView view,
            IEnumerable<IConfiguration> branchCaseSections,
            Action<IBranchCase>? configureBranchCase = null)
        {
            view.Services.TryAddSingleton<IConfiguredVersionIncrementFlowFactory, ConfiguredVersionIncrementFlowFactory>();
            view.Services.TryAddSingleton<IConfiguredMessageConventionFactory, ConfiguredMessageConventionFactory>();
            view.Services.TryAddSingleton<IConfiguredVersioningPresetFactory, ConfiguredVersioningPresetFactory>();
            return view.AddBranchCases(branchCaseSections.Select(configuration => CreateBranchCase(configuration, configureBranchCase)));
        }

        /// <summary>
        /// Adds multiple branch cases from configuration sections.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="branchCaseSection"></param>
        /// <param name="addtionalBranchCaseSections"></param>
        /// <param name="configureBranchCase"></param>
        public static IGitServicesView AddBranchCases(
            this IGitServicesView view,
            IConfiguration branchCaseSection,
            IEnumerable<IConfiguration> addtionalBranchCaseSections,
            Action<IBranchCase>? configureBranchCase = null)
        {
            static IEnumerable<IConfiguration> GetConfigurations(IConfiguration configuration, IEnumerable<IConfiguration> addtionalSections)
            {
                yield return configuration;

                foreach (var additionalSection in addtionalSections) {
                    yield return additionalSection;
                }
            }

            return view.AddBranchCases(GetConfigurations(branchCaseSection, addtionalBranchCaseSections), configureBranchCase);
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

        /// <summary>
        /// Creates versioning mode extension for each branch case and
        /// configures <see cref="VersionIncrementationOptions"/> to set
        /// <see cref="VersionIncrementationOptions.VersioningPreset"/>
        /// from active branch case.
        /// </summary>
        /// <param name="view"></param>
        public static IGitServicesView UseActiveBranchCaseVersioningMode(this IGitServicesView view)
        {
            view.ConfigureBranchCases(branchCases => branchCases.ForEachSetVersioningPresetExtensionFactory());

            view.Services.AddOptions<VersionIncrementationOptions>()
                .Configure<IBranchCasesProvider>((options, branchCaseProvider) =>
                    options.VersioningPreset = branchCaseProvider.ActiveBranchCase.GetVersioningPresetExtension());

            return view;
        }

        /// <summary>
        /// Configures an instance of <see cref="IBranchCasesServicesView"/>.
        /// </summary>
        /// <param name="view">
        /// <param name="configure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IGitServicesView ConfigureBranchCases(this IGitServicesView view, Action<IBranchCasesServicesView> configure)
        {
            var services = view.Services;

            if (configure is null) {
                throw new ArgumentNullException(nameof(configure));
            }

            var options = new BranchCasesServicesView(services);
            configure(options);
            return view;
        }
    }
}
