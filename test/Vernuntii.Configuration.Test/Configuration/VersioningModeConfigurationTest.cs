using Microsoft.Extensions.Configuration;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.VersionIncrementing;
using Vernuntii.VersioningPresets;
using Xunit;
using static Vernuntii.Extensions.ServiceCollectionFixture;

namespace Vernuntii.Configuration
{
    public class VersioningModeConfigurationTest
    {
        private const string VersioningModeStringFileName = "string.yml";
        private const string VersioningModeObjectInvalidFileName = "object-invalid.yml";
        private const string VersioningModeObjectValidFileName = "object-valid.yml";

        public static IEnumerable<object[]> VersioningModeStringShouldMatchPresetGenerator()
        {
            foreach (var value in CreateBranchCasesProvider(VersioningModeDirectory, VersioningModeStringFileName, tryCreateVersioningPresetExtension: true).BranchCases.Values) {
                yield return new object[] {
                     DefaultPresetManager.VersioningPresets.GetItem(value.GetConfigurationExtension().GetValue<string>(ConfiguredVersioningPresetFactory.DefaultVersioningModeKey)
                        ?? nameof(InbuiltVersioningPreset.Default)),
                     value.GetVersioningPresetExtension()
                 };
            }
        }

        [Theory]
        [MemberData(nameof(VersioningModeStringShouldMatchPresetGenerator))]
        public void VersioningModeStringShouldMatchPreset(
            IVersioningPreset expectedExtensionOptions,
            IVersioningPreset assumedExtensionOptions) =>
            Assert.Equal(expectedExtensionOptions, assumedExtensionOptions);

        public static IEnumerable<object[]> InvalidVersioningModeObjectShouldThrowGenerator()
        {
            var branchCases = CreateBranchCasesProvider(VersioningModeDirectory, VersioningModeObjectInvalidFileName).NestedBranchCases;

            yield return new object[] {
                 nameof(IVersioningPreset.IncrementMode),
                 () => CreateVersioningModeExtension(branchCases["OnlyConvention"])
             };

            yield return new object[] {
                 nameof(IVersioningPreset.MessageConvention),
                 () => CreateVersioningModeExtension(branchCases["OnlyIncrementMode"])
             };

            static IVersioningPreset CreateVersioningModeExtension(IBranchCase branchCase) => branchCase
                .SetVersioningPresetExtensionFactory(DefaultConfiguredVersioningPresetFactory)
                .GetVersioningPresetExtension();
        }

        [Theory]
        [MemberData(nameof(InvalidVersioningModeObjectShouldThrowGenerator))]
        public void InvalidVersioningModeObjectShouldThrow(
            string expectedArgumentExceptionFieldName,
            Func<IVersioningPreset> presetExtensionFactory)
        {
            var error = Record.Exception(presetExtensionFactory);
            var argumentException = Assert.IsType<ConfigurationValidationException>(error);
            Assert.Contains(expectedArgumentExceptionFieldName, argumentException.Message, StringComparison.Ordinal);
        }

        public static IEnumerable<object[]> ValidVersioningModeObjectShouldMatchGenerator()
        {
            var branchCases = CreateBranchCasesProvider(VersioningModeDirectory, VersioningModeObjectValidFileName, tryCreateVersioningPresetExtension: true).NestedBranchCases;

            yield return new object[] {
                 VersioningPreset.ConventionalCommitsDelivery,
                 branchCases["OnlyPreset"].GetVersioningPresetExtension()
             };

            {
                yield return new object[] {
                     VersioningPreset.Manual with {
                         MessageConvention = VersioningPreset.ConventionalCommitsDelivery.MessageConvention,
                         IncrementMode = VersionIncrementMode.Successive
                     },
                     branchCases["Mixing"].GetVersioningPresetExtension()
                 };
            }
        }

        [Theory]
        [MemberData(nameof(ValidVersioningModeObjectShouldMatchGenerator))]
        public void ValidVersioningModeObjectShouldMatch(
            IVersioningPreset expectedExtensionOptions,
            IVersioningPreset assumedExtensionOptions) =>
            Assert.Equal(expectedExtensionOptions, assumedExtensionOptions);
    }
}
