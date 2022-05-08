using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vernuntii.Configuration;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.MessageConventions;
using Vernuntii.PluginSystem;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions
{
    internal static class ServiceCollectionFixture
    {
        public readonly static VersioningPresetsPlugin DefaultVersioningPresetsPlugin = new VersioningPresetsPlugin();
        public readonly static IVersioningPresetManager DefaultPresetManager = DefaultVersioningPresetsPlugin.PresetManager;
        public readonly static ConfiguredMessageConventionFactory DefaultConfiguredMessageConventionFactory = new ConfiguredMessageConventionFactory(DefaultPresetManager);

        public readonly static ConfiguredVersioningPresetFactory DefaultConfiguredVersioningPresetFactory = new ConfiguredVersioningPresetFactory(
            DefaultPresetManager,
            DefaultConfiguredMessageConventionFactory);

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
            var services = CreateBranchCasesProviderServices(directory, fileName);
            ConfigureServiceCollection(services, tryCreateVersioningPresetExtension: tryCreateVersioningPresetExtension);
            return CreateBranchCasesProvider(services);
        }
    }
}
