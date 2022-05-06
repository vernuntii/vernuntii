using Microsoft.Extensions.Configuration;
using Vernuntii.MessageVersioning;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IBranchCase"/>.
    /// </summary>
    public static class BranchCaseExtensions
    {
        /// <summary>
        /// Gets property or creates it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="branchCaseArguments"></param>
        /// <param name="extensionName"></param>
        /// <returns>The existing or created property.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T GetExtensionOrCreate<T>(this IBranchCase branchCaseArguments, string extensionName)
            where T : new()
        {
            if (!branchCaseArguments.Extensions.TryGetValue(extensionName, out var propertyObject)) {
                var newProperty = new T();
                branchCaseArguments.Extensions.Add(extensionName, newProperty);
                return newProperty;
            } else if (propertyObject is T property) {
                return property;
            } else {
                throw new InvalidOperationException($"The existing property is of type {propertyObject.GetType().FullName} but {typeof(T).FullName} was expected");
            }
        }

        /// <summary>
        /// Gets property or creates it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="branchCaseArguments"></param>
        /// <param name="extensionName"></param>
        /// <param name="extensionFactory"></param>
        /// <returns>The existing or created property.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T GetExtensionOrCreate<T>(this IBranchCase branchCaseArguments, string extensionName, Func<T> extensionFactory)
            where T : notnull
        {
            if (!branchCaseArguments.Extensions.TryGetValue(extensionName, out var propertyObject)) {
                var newProperty = extensionFactory();
                branchCaseArguments.Extensions.Add(extensionName, newProperty);
                return newProperty;
            } else if (propertyObject is T property) {
                return property;
            } else {
                throw new InvalidOperationException($"The existing property is of type {propertyObject.GetType().FullName} but {typeof(T).FullName} was expected");
            }
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
            if (branchCaseArguments.Extensions.TryGetValue(extensionName, out var extensionObject) && extensionObject is T extension) {
                return extension;
            } else {
                throw new InvalidOperationException($"The existing \"{extensionName}\" of type {typeof(T).FullName} was expected");
            }
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
        /// Binds configuration to new or existing instance of type <see cref="VersioningPresetExtension"/>.
        /// </summary>
        /// <param name="branchCaseArguments"></param>
        /// <param name="presetExtensionFactory"></param>
        /// <param name="branchConfiguration"></param>
        public static IBranchCase TryCreateVersioningPresetExtension(this IBranchCase branchCaseArguments, IVersioningPresetExtensionFactory presetExtensionFactory, IConfiguration? branchConfiguration = null)
        {
            branchConfiguration ??= branchCaseArguments.GetConfigurationExtension();

            _ = branchCaseArguments.GetExtensionOrCreate(
                VersioningPresetExtension.ExtensionName,
                () => presetExtensionFactory.Create(branchConfiguration));

            return branchCaseArguments;
        }

        /// <summary>
        /// Gets the instance of <see cref="VersioningPresetExtension"/> from <see cref="IBranchCase.Extensions"/>.
        /// </summary>
        /// <param name="branchCaseArguments"></param>
        public static IVersioningPresetExtension GetVersioningModeExtension(this IBranchCase branchCaseArguments) =>
            branchCaseArguments.GetExtension<IVersioningPresetExtension>(VersioningPresetExtension.ExtensionName);

        internal class VersioningModeObject
        {
            public VersionIncrementMode? IncrementMode { get; set; }
            public string? Preset { get; set; }
            public bool RightShiftWhenZeroMajor { get; set; }
            public string? MessageConvention { get; set; }
        }

        #endregion
    }
}
