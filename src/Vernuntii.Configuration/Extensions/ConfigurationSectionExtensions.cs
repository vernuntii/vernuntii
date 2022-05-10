using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IConfigurationSection"/>.
    /// </summary>
    internal static class ConfigurationSectionExtensions
    {
        /// <summary>
        /// Checking if section does not exist and does have a value.
        /// </summary>
        /// <param name="configurationSection"></param>
        /// <param name="value"></param>
        /// <returns><see langword="true"/> if section does not exist or has a value.</returns>
        public static bool Value(this IConfigurationSection configurationSection, [NotNullWhen(true)] out string? value)
        {
            if (configurationSection.Exists() && configurationSection.Value != null) {
                value = configurationSection.Value;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Checking if section does not exist and does not have a value.
        /// </summary>
        /// <param name="configurationSection"></param>
        /// <returns><see langword="true"/> if section does not exist and does have a value.</returns>
        public static bool NotExisting(this IConfigurationSection? configurationSection) =>
            configurationSection == null || !configurationSection.Exists();

        /// <summary>
        /// Checking if section does not exist and does have a value.
        /// </summary>
        /// <param name="configurationSection"></param>
        /// <param name="value"></param>
        /// <returns><see langword="true"/> if section does not exist or has a value.</returns>
        public static bool NotExistingOrValue(this IConfigurationSection configurationSection, out string? value)
        {
            if (configurationSection.NotExisting()) {
                value = null;
                return true;
            }

            return configurationSection.Value(out value);
        }
    }
}
