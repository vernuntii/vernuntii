using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration.Yaml;

namespace Vernuntii.Configuration
{
    /// <summary>
    /// Extension methods for <see cref="IConventionalConfigurationBuilder"/>.
    /// </summary>
    public static class ConventionalConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds messages versioning configuration file.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="directoryName"></param>
        /// <param name="fileName"></param>
        public static IConventionalConfigurationBuilder AddUpwardYamlFile(
            this IConventionalConfigurationBuilder builder,
            string directoryName,
            string fileName = YamlConfigurationFileDefaults.YmlFileName)
        {
            builder.AddYamlFile(builder.FileFinder
                .FindFile(directoryName, fileName)
                .GetUpwardFilePath());

            return builder;
        }

        /// <summary>
        /// Adds an instance of <see cref="YamlConfigurationFileFinder"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder"></param>
        public static IConventionalConfigurationBuilder AddConventionalYamlFileFinder(this IConventionalConfigurationBuilder builder)
        {
            builder.AddConventionalFileFinder(new YamlConfigurationFileFinder(builder.FileFinder));
            return builder;
        }
    }
}
