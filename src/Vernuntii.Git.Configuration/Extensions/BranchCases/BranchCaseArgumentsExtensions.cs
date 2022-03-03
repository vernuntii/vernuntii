using Microsoft.Extensions.Configuration;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IBranchCaseArguments"/>.
    /// </summary>
    public static class BranchCaseArgumentsExtensions
    {
        /// <summary>
        /// Gets property or creates it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="branchCaseArguments"></param>
        /// <param name="extensionName"></param>
        /// <returns>The existing or created property.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T GetExtensionOrCreate<T>(this IBranchCaseArguments branchCaseArguments, string extensionName)
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
        public static T GetExtensionOrCreate<T>(this IBranchCaseArguments branchCaseArguments, string extensionName, Func<T> extensionFactory)
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
        public static T GetExtension<T>(this IBranchCaseArguments branchCaseArguments, string extensionName)
        {
            if (branchCaseArguments.Extensions.TryGetValue(extensionName, out var extensionObject) && extensionObject is T extension) {
                return extension;
            } else {
                throw new InvalidOperationException($"The existing \"{extensionName}\" of type {typeof(T).FullName} was expected");
            }
        }

        /// <summary>
        /// Gets the configuration from <see cref="IBranchCaseArguments.Extensions"/>.
        /// </summary>
        /// <param name="branchCaseArguments"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        public static IConfiguration GetConfigurationExtension(this IBranchCaseArguments branchCaseArguments) =>
            branchCaseArguments.GetExtension<IConfiguration>(BranchCaseArguments.ConfigurationExtensionName);
    }
}
