using Microsoft.Extensions.DependencyInjection;
using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.MessageConventions.MessageIndicators;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.VersionIncrementFlows;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// A plugin to manage versioning presets.
    /// </summary>
    public class VersioningPresetsPlugin : Plugin, IVersioningPresetsPlugin
    {
        /// <summary>
        /// Creates a default preset compendium.
        /// </summary>
        private static VersioningPresetManager CreateDefault()
        {
            var presets = new VersioningPresetManager();

            // Adds presets. Maps also to increment flows, message conventions and height conventions.
            presets.Add(nameof(InbuiltVersioningPreset.Default), VersioningPreset.Default);
            presets.Add(nameof(InbuiltVersioningPreset.Manual), VersioningPreset.Manual);
            presets.Add(nameof(InbuiltVersioningPreset.ContinousDelivery), VersioningPreset.ContinousDelivery);
            presets.Add(nameof(InbuiltVersioningPreset.ContinousDeployment), VersioningPreset.ContinousDeployment);
            presets.Add(nameof(InbuiltVersioningPreset.ConventionalCommitsDelivery), VersioningPreset.ConventionalCommitsDelivery);
            presets.Add(nameof(InbuiltVersioningPreset.ConventionalCommitsDeployment), VersioningPreset.ConventionalCommitsDeployment);

            // Adds version increment flows.
            presets.IncrementFlows.AddItem(nameof(InbuiltVersionIncrementFlow.None), VersionIncrementFlow.None);
            presets.IncrementFlows.AddItem(nameof(InbuiltVersionIncrementFlow.ZeroMajorDownstream), VersionIncrementFlow.ZeroMajorDownstream);

            // Add message conventions.
            presets.MessageConventions.AddItem(nameof(InbuiltMessageConvention.Continous), VersioningPreset.ContinousDelivery.MessageConvention);
            presets.MessageConventions.AddItem(nameof(InbuiltMessageConvention.ConventionalCommits), VersioningPreset.ConventionalCommitsDelivery.MessageConvention);

            // Add message indicators.
            presets.MessageIndicators.AddItem(nameof(InbuiltMessageIndicator.Falsy), FalsyMessageIndicator.Default);
            presets.MessageIndicators.AddItem(nameof(InbuiltMessageIndicator.Truthy), TruthyMessageIndicator.Default);
            presets.MessageIndicators.AddItem(nameof(InbuiltMessageIndicator.ConventionalCommits), ConventionalCommitsMessageIndicator.Default);

            // Add configured message indicator factories.
            presets.ConfiguredMessageIndicatorFactories.AddItem(nameof(InbuiltConfiguredMessageIndicatorFactory.Regex), ConfiguredRegexMessageIndicatorFactory.Default);

            presets.HeightConventions.AddItem(nameof(InbuiltHeightConvention.None), HeightConvention.None);
            presets.HeightConventions.AddItem(nameof(InbuiltHeightConvention.InPreReleaseAfterFirstDot), HeightConvention.InPreReleaseAfterFirstDot);

            return presets;
        }

        /// <inheritdoc/>
        public override int? Order => -4000;

        /// <summary>
        /// <inheritdoc/>
        /// Includes initially all inbuilt presets and others.
        /// </summary>
        public IVersioningPresetManager PresetManager => _presetManager ??= CreateDefault();

        private IVersioningPresetManager? _presetManager;

        /// <summary>
        /// Creates an instance of this instance and poplulates <see cref="PresetManager"/> with defaults.
        /// </summary>
        public VersioningPresetsPlugin()
        {
        }

        /// <summary>
        /// <inheritdoc/> Registers <see cref="PresetManager"/> to global services as
        /// <br/> - <see cref="IVersioningPresetManager"/>
        /// <br/> - <see cref="IVersioningPresetRegistry"/>
        /// <br/> - <see cref="IMessageConventionRegistry"/>
        /// <br/> - <see cref="IMessageIndicatorRegistry"/>
        /// <br/> - <see cref="IConfiguredMessageIndicatorFactoryRegistry"/>
        /// <br/> - <see cref="IHeightConventionRegistry"/>
        /// </summary>
        protected override void OnEvents()
        {
            Events.SubscribeOnce(NextVersionEvents.ConfigureGlobalServices, services => {
                services.AddSingleton(PresetManager);
                services.AddSingleton(PresetManager.VersioningPresets);
                services.AddSingleton(PresetManager.MessageConventions);
                services.AddSingleton(PresetManager.MessageIndicators);
                services.AddSingleton(PresetManager.ConfiguredMessageIndicatorFactories);
                services.AddSingleton(PresetManager.HeightConventions);
                Events.Publish(VersioningPresetsEvents.ConfiguredGlobalServices, services);
            });
        }
    }
}
