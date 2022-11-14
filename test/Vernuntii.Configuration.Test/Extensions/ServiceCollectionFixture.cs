using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vernuntii.Configuration;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.MessageConventions;
using Vernuntii.Plugins;
using Vernuntii.VersionIncrementFlows;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions
{
    internal static class ServiceCollectionFixture
    {
        public static readonly VersioningPresetsPlugin DefaultVersioningPresetsPlugin = new();
        public static readonly IVersioningPresetManager DefaultPresetManager = DefaultVersioningPresetsPlugin.PresetManager;
        public static readonly ConfiguredMessageConventionFactory DefaultConfiguredMessageConventionFactory = new(DefaultPresetManager);
        public static readonly ConfiguredVersionIncrementFlowFactory DefaultConfiguredIncrementFlowFactory = new(DefaultPresetManager);

        public static readonly ConfiguredVersioningPresetFactory DefaultConfiguredVersioningPresetFactory = new(
            DefaultPresetManager,
            DefaultConfiguredMessageConventionFactory,
            DefaultConfiguredIncrementFlowFactory);

        public static IServiceCollection ConfigureServiceCollection(
            IServiceCollection services,
            IConfiguration? gitConfiguration = null,
            IVersioningPresetManager? presetManager = null,
            bool tryCreateVersioningPresetExtension = false)
        {
            services
                .AddLogging()
                .AddOptions()
                .TryAddSingleton(presetManager ?? DefaultPresetManager);

            if (gitConfiguration != null) {
                services
                    .ConfigureVernuntii(features => features
                    .ConfigureGit(features => features
                        .AddRepository()
                        .UseConfigurationDefaults(gitConfiguration)));
            }

            if (tryCreateVersioningPresetExtension) {
                services
                    .ConfigureVernuntii(features => features
                        .ConfigureGit(features => features
                            .ConfigureBranchCases(branchCases => branchCases
                            .TryCreateVersioningPresetExtension())));
            }

            return services;
        }

        private static IBranchCasesProvider CreateBranchCasesProvider(IServiceCollection services) =>
            services.BuildLifetimeScopedServiceProvider().CreateScope().ServiceProvider.GetRequiredService<IBranchCasesProvider>();

        private static IServiceCollection CreateBranchCasesProviderServices(string directory, string fileName) =>
            ConfigureServiceCollection(
                new ServiceCollection(),
                gitConfiguration: ConfigurationFixture.Default.FindYamlConfigurationFile(directory, fileName));

        public static IBranchCasesProvider CreateBranchCasesProvider(string directory, string fileName, bool tryCreateVersioningPresetExtension = false)
        {
            IServiceCollection services = CreateBranchCasesProviderServices(directory, fileName);
            ConfigureServiceCollection(services, tryCreateVersioningPresetExtension: tryCreateVersioningPresetExtension);
            return CreateBranchCasesProvider(services);
        }
    }
}
