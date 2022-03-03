using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Extensions;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.MessageVersioning;
using Xunit;

namespace Vernuntii.Configuration
{
    public class VersioningModeConfigurationTest
    {
        private const string VersioningModeStringFileName = "String.yml";
        private const string VersioningModeObjectInvalidFileName = "ObjectInvalid.yml";
        private const string VersioningModeObjectValidFileName = "ObjectValid.yml";

        private static AnyPath Workspace = FilesystemDir / "versioning-mode";

        private static IServiceCollection CreateBranchCasesProviderServices(string fileName) =>
            new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .ConfigureVernuntii(features => features
                    .ConfigureGit(features => features
                        .UseConfigurationDefaults(ConfigurationFixture.Default.FindYamlConfigurationFile(Workspace, fileName))));

        private static IBranchCaseArgumentsProvider CreateBranchCasesProvider(IServiceCollection services) =>
            services.BuildLifetimeScopedServiceProvider().CreateScope().ServiceProvider.GetRequiredService<IBranchCaseArgumentsProvider>();

        private static IBranchCaseArgumentsProvider CreateBranchCasesProvider(string fileName)
        {
            var services = CreateBranchCasesProviderServices(fileName);

            services.ConfigureVernuntii(features => features
                .ConfigureGit(features => features
                    .ConfigureBranchCases(branchCase => branchCase.TryCreateVersioningModeExtension())));

            return CreateBranchCasesProvider(services);
        }

        public static IEnumerable<object[]> VersioningModeStringShouldMatchPresetGenerator()
        {
            foreach (var value in CreateBranchCasesProvider(VersioningModeStringFileName).BranchCases.Values) {
                yield return new object[] {
                    MessageVersioningModeExtensionOptions.WithPreset(value.GetConfigurationExtension().GetValue<string>(CalculatorBranchCaseArgumentsExtensions.VersioningModeKey)),
                    value.GetVersioningModeExtension()
                };
            }
        }

        [Theory]
        [MemberData(nameof(VersioningModeStringShouldMatchPresetGenerator))]
        public void VersioningModeStringShouldMatchPreset(
            MessageVersioningModeExtensionOptions expectedExtensionOptions,
            MessageVersioningModeExtensionOptions assumedExtensionOptions) =>
            Assert.Equal(expectedExtensionOptions, assumedExtensionOptions);

        public static IEnumerable<object[]> InvalidVersioningModeObjectShouldThrowGenerator()
        {
            var branchCases = CreateBranchCasesProvider(CreateBranchCasesProviderServices(VersioningModeObjectInvalidFileName)).NestedBranchCases;

            yield return new object[] {
                nameof(CalculatorBranchCaseArgumentsExtensions.VersioningModeObject.IncrementMode),
                () => CreateVersioningModeExtension(branchCases["OnlyConvention"])
            };

            yield return new object[] {
                nameof(CalculatorBranchCaseArgumentsExtensions.VersioningModeObject.MessageConvention),
                () => CreateVersioningModeExtension(branchCases["OnlyIncrementMode"])
            };

            static MessageVersioningModeExtensionOptions CreateVersioningModeExtension(IBranchCaseArguments branchCase) => branchCase
                .TryCreateVersioningModeExtension()
                .GetVersioningModeExtension();
        }

        [Theory]
        [MemberData(nameof(InvalidVersioningModeObjectShouldThrowGenerator))]
        public void InvalidVersioningModeObjectShouldThrow(
            string expectedArgumentExceptionFieldName,
            Func<MessageVersioningModeExtensionOptions> extensionFactory)
        {
            var error = Record.Exception(extensionFactory);
            var argumentException = Assert.IsType<ArgumentException>(error);
            Assert.Contains(expectedArgumentExceptionFieldName, argumentException.Message);
        }

        public static IEnumerable<object[]> ValidVersioningModeObjectShouldMatchGenerator()
        {
            var branchCases = CreateBranchCasesProvider(VersioningModeObjectValidFileName).NestedBranchCases;

            yield return new object[] {
                MessageVersioningModeExtensionOptions.WithPreset(VersioningModePreset.ConventionalCommitsDelivery),
                branchCases["OnlyPreset"].GetVersioningModeExtension()
            };

            {
                var expectedExtensionOptions = MessageVersioningModeExtensionOptions.WithPreset(VersioningModePreset.Manual);

                yield return new object[] {
                    expectedExtensionOptions with {
                        VersionTransformerOptions = expectedExtensionOptions.VersionTransformerOptions.WithConvention(VersioningModeMessageConvention.ConventionalCommits) with {
                            IncrementMode = VersionIncrementMode.Successive
                        }
                    },
                    branchCases["Mixing"].GetVersioningModeExtension()
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidVersioningModeObjectShouldMatchGenerator))]
        public void ValidVersioningModeObjectShouldMatch(
            MessageVersioningModeExtensionOptions expectedExtensionOptions,
            MessageVersioningModeExtensionOptions assumedExtensionOptions) =>
            Assert.Equal(expectedExtensionOptions, assumedExtensionOptions);
    }
}
