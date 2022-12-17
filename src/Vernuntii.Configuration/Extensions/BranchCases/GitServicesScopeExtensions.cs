using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vernuntii.MessageConventions;
using Vernuntii.VersionIncrementFlows;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IGitServicesScope"/>
    /// </summary>
    public static class GitServicesScopeExtensions
    {
        private static BranchCase CreateBranchCase(IConfiguration configuration, Action<IBranchCase>? configureBranchCase)
        {
            var branchCaseArguments = new BranchCase();
            configuration.Bind(branchCaseArguments);
            branchCaseArguments.Extensions[BranchCase.ConfigurationExtensionName] = configuration;
            configureBranchCase?.Invoke(branchCaseArguments);
            return branchCaseArguments;
        }

        /// <summary>
        /// Adds multiple branch cases from configuration sections.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="branchCaseSections"></param>
        /// <param name="configureBranchCase"></param>
        public static IGitServicesScope AddBranchCases(
            this IGitServicesScope scope,
            IEnumerable<IConfiguration> branchCaseSections,
            Action<IBranchCase>? configureBranchCase = null)
        {
            scope.Services.TryAddSingleton<IConfiguredVersionIncrementFlowFactory, ConfiguredVersionIncrementFlowFactory>();
            scope.Services.TryAddSingleton<IConfiguredMessageConventionFactory, ConfiguredMessageConventionFactory>();
            scope.Services.TryAddSingleton<IConfiguredVersioningPresetFactory, ConfiguredVersioningPresetFactory>();
            return scope.AddBranchCases(branchCaseSections.Select(configuration => CreateBranchCase(configuration, configureBranchCase)));
        }

        /// <summary>
        /// Adds multiple branch cases from configuration sections.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="branchCaseSection"></param>
        /// <param name="addtionalBranchCaseSections"></param>
        /// <param name="configureBranchCase"></param>
        public static IGitServicesScope AddBranchCases(
            this IGitServicesScope scope,
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

            return scope.AddBranchCases(GetConfigurations(branchCaseSection, addtionalBranchCaseSections), configureBranchCase);
        }

        /// <summary>
        /// Creates versioning mode extension for each branch case and
        /// configures <see cref="VersionIncrementationOptions"/> to set
        /// <see cref="VersionIncrementationOptions.VersioningPreset"/>
        /// from active branch case.
        /// </summary>
        /// <param name="scope"></param>
        public static IGitServicesScope UseActiveBranchCaseVersioningMode(this IGitServicesScope scope)
        {
            scope.ConfigureBranchCases(branchCases => branchCases.TryCreateVersioningPresetExtension());

            scope.Services.AddOptions<VersionIncrementationOptions>()
                .Configure<IBranchCasesProvider>((options, branchCaseProvider) =>
                    options.VersioningPreset = branchCaseProvider.ActiveBranchCase.GetVersioningPresetExtension());

            return scope;
        }
    }
}
