using Microsoft.Extensions.Configuration;

namespace Vernuntii.Configuration
{
    /// <summary>
    /// Extension methods for <see cref="IConfiguration"/>.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Creates a section provider
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="defaultKey"></param>
        public static IDefaultConfigurationSectionProvider GetSectionProvider(this IConfiguration configuration, string defaultKey) =>
            new DefaultConfigurationSectionProvider(configuration, defaultKey);
    }
}
