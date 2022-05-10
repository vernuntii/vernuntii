using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IBranchCase"/>.
    /// </summary>
    public static class BranchCaseExtensions
    {
        /// <summary>
        /// Sets extension factory that is used when getting the extension the first time.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="branchCaseArguments"></param>
        /// <param name="extensionName"></param>
        /// <param name="extensionFactory"></param>
        /// <returns>The existing or created property.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IBranchCase SetExtensionFactory<T>(this IBranchCase branchCaseArguments, string extensionName, Func<T> extensionFactory)
            where T : notnull
        {
            if (branchCaseArguments.Extensions.ContainsKey(extensionName)) {
                throw new InvalidOperationException($"Extenion \"{extensionName}\" has been already created");
            }

            branchCaseArguments.Extensions.Add(extensionName, extensionFactory);
            return branchCaseArguments;
        }

        /// <summary>
        /// Gets extension.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="branchCaseArguments"></param>
        /// <param name="extensionName"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static T GetExtension<T>(this IBranchCase branchCaseArguments, string extensionName)
        {
            if (branchCaseArguments.Extensions.TryGetValue(extensionName, out var extensionObject)) {
                if (extensionObject is T extension) {
                    return extension;
                } else if (extensionObject is Func<T> extensionFactory) {
                    extension = extensionFactory();
                    branchCaseArguments.Extensions[extensionName] = extension;
                    return extension;
                }
            }

            throw new InvalidOperationException($"The existing \"{extensionName}\" of type {typeof(T).FullName} was expected");
        }

        /// <summary>
        /// Gets the configuration from <see cref="IBranchCase.Extensions"/>.
        /// </summary>
        /// <param name="branchCaseArguments"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        public static IConfiguration GetConfigurationExtension(this IBranchCase branchCaseArguments) =>
            branchCaseArguments.GetExtension<IConfiguration>(BranchCase.ConfigurationExtensionName);

        #region VersioningModeExtension

        /// <summary>
        /// Binds configuration to new or existing instance of type <see cref="IVersioningPreset"/>.
        /// </summary>
        /// <param name="branchCaseArguments"></param>
        /// <param name="presetFactory"></param>
        /// <param name="branchConfiguration"></param>
        public static IBranchCase SetVersioningPresetExtensionFactory(this IBranchCase branchCaseArguments, IConfiguredVersioningPresetFactory presetFactory, IConfiguration? branchConfiguration = null)
        {
            branchConfiguration ??= branchCaseArguments.GetConfigurationExtension();

            branchCaseArguments.SetExtensionFactory(
                nameof(IVersioningPreset),
                () => presetFactory.Create(branchConfiguration.GetSectionProvider(ConfiguredVersioningPresetFactory.DefaultVersioningModeKey)));

            return branchCaseArguments;
        }

        /// <summary>
        /// Gets the instance of <see cref="IVersioningPreset"/> from <see cref="IBranchCase.Extensions"/>.
        /// </summary>
        /// <param name="branchCaseArguments"></param>
        public static IVersioningPreset GetVersioningPresetExtension(this IBranchCase branchCaseArguments) =>
            branchCaseArguments.GetExtension<IVersioningPreset>(nameof(IVersioningPreset));

        #endregion
    }
}
