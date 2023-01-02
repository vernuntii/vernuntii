using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vernuntii.Configuration;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.Git;
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
            bool setVersioningPresetExtension = false,
            bool allowShallowRepository = true)
        {
            services
                .AddLogging()
                .AddOptions()
                .TryAddSingleton(presetManager ?? DefaultPresetManager);

            if (allowShallowRepository) {
                services.Configure<RepositoryOptions>(options => options.AllowShallow = true);
            }

            services
                .TakeViewOfVernuntii()
                .TakeViewOfGit()
                .AddRepository();

            if (gitConfiguration != null) {
                services
                    .TakeViewOfVernuntii()
                    .TakeViewOfGit()
                    .UseConfigurationDefaults(gitConfiguration);
            }

            if (setVersioningPresetExtension) {
                services
                    .TakeViewOfVernuntii()
                    .TakeViewOfGit()
                    .ConfigureBranchCases(branchCases => branchCases
                    .ForEachSetVersioningPresetExtensionFactory());
            }

            return services;
        }

        public static IBranchCasesProvider CreateBranchCasesProvider(string directory, string fileName, bool setVersioningPresetExtension = false)
        {
            var services = ConfigureServiceCollection(
                new ServiceCollection(),
                gitConfiguration: ConfigurationFixture.Default.FindYamlConfigurationFile(directory, fileName),
                setVersioningPresetExtension: setVersioningPresetExtension, allowShallowRepository: true);

            return services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<IBranchCasesProvider>();
        }
    }
}
