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
        /// <returns><see langword="true"/> if section does not exist and has a value.</returns>
        public static bool HavingValue(this IConfigurationSection? configurationSection) =>
            configurationSection == null || !configurationSection.Exists() || configurationSection.Value != null;

        /// <summary>
        /// Checking if section does not exist and does not have a value.
        /// </summary>
        /// <param name="configurationSection"></param>
        /// <returns><see langword="true"/> if section does not exist and does have a value.</returns>
        public static bool NotExisting(this IConfigurationSection? configurationSection) =>
            configurationSection == null || (!configurationSection.Exists() && configurationSection.Value == null);
    }
}
