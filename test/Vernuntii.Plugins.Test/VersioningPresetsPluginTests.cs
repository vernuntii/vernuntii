using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.MessageConventions.MessageIndicators;
using Vernuntii.VersionIncrementFlows;
using Vernuntii.VersioningPresets;
using Xunit;

namespace Vernuntii.Plugins
{
    public class VersioningPresetsPluginTests
    {
        private static readonly VersioningPresetsPlugin DefaultPlugin = new();

        public static IEnumerable<object[]> DefaultPresetNames()
        {
            foreach (var name in Enum.GetNames<InbuiltVersioningPreset>()) {
                yield return new[] { name };
            }
        }

        [Theory]
        [MemberData(nameof(DefaultPresetNames))]
        public void PresetManagerShouldHaveDefaultVersioningPresets(string presetName) =>
            Assert.True(DefaultPlugin.PresetManager.VersioningPresets.ContainsName(presetName));

        public static IEnumerable<object[]> DefaultIncrementFlowNames()
        {
            foreach (var name in Enum.GetNames<InbuiltVersionIncrementFlow>()) {
                yield return new[] { name };
            }
        }

        [Theory]
        [MemberData(nameof(DefaultPresetNames))]
        [MemberData(nameof(DefaultIncrementFlowNames))]
        public void PresetManagerShouldHaveDefaultIncrementFlows(string incrementFlowName) =>
            Assert.True(DefaultPlugin.PresetManager.IncrementFlows.ContainsName(incrementFlowName));

        public static IEnumerable<object[]> DefaultMessageConventionNames()
        {
            foreach (var name in Enum.GetNames<InbuiltMessageConvention>()) {
                yield return new[] { name };
            }
        }

        [Theory]
        [MemberData(nameof(DefaultPresetNames))]
        [MemberData(nameof(DefaultMessageConventionNames))]
        public void PresetManagerShouldHaveDefaultMessageConventions(string messageConventionName) =>
            Assert.True(DefaultPlugin.PresetManager.MessageConventions.ContainsName(messageConventionName));

        public static IEnumerable<object[]> DefaultHeightConventionNames()
        {
            foreach (var name in Enum.GetNames<InbuiltHeightConvention>()) {
                yield return new[] { name };
            }
        }

        [Theory]
        [MemberData(nameof(DefaultPresetNames))]
        [MemberData(nameof(DefaultHeightConventionNames))]
        public void PresetManagerShouldHaveDefaultHeightConventions(string heightConventionName) =>
            Assert.True(DefaultPlugin.PresetManager.HeightConventions.ContainsName(heightConventionName));

        public static IEnumerable<object[]> DefaultMessageIndicatorNames()
        {
            foreach (var name in Enum.GetNames<InbuiltMessageIndicator>()) {
                yield return new[] { name };
            }
        }

        [Theory]
        [MemberData(nameof(DefaultMessageIndicatorNames))]
        public void PresetManagerShouldHaveDefaultMessageIndicators(string messageIndicatorName) =>
            Assert.True(DefaultPlugin.PresetManager.MessageIndicators.ContainsName(messageIndicatorName));

        public static IEnumerable<object[]> DefaultConfiguredMessageIndicatorFactoryNames()
        {
            foreach (var name in Enum.GetNames<InbuiltConfiguredMessageIndicatorFactory>()) {
                yield return new[] { name };
            }
        }

        [Theory]
        [MemberData(nameof(DefaultConfiguredMessageIndicatorFactoryNames))]
        public void PresetManagerShouldHaveDefaultConfiguredMessageIndicatorFactories(string configuredMessageIndicatorFactoryName) =>
            Assert.True(DefaultPlugin.PresetManager.ConfiguredMessageIndicatorFactories.ContainsName(configuredMessageIndicatorFactoryName));
    }
}
