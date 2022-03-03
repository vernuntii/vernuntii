using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration.Json;

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
        /// <param name="configurationBuilder"></param>
        /// <param name="directoryName"></param>
        /// <param name="fileName"></param>
        public static IConventionalConfigurationBuilder AddUpwardJsonFile(
            this IConventionalConfigurationBuilder configurationBuilder,
            string directoryName,
            string fileName = JsonConfigurationFileDefaults.JsonFileName)
        {
            configurationBuilder.AddJsonFile(configurationBuilder.FileFinder
                .FindFile(directoryName, fileName)
                .GetUpwardFilePath());

            return configurationBuilder;
        }

        /// <summary>
        /// Adds an instance of <see cref="JsonConfigurationFileFinder"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder"></param>
        public static IConventionalConfigurationBuilder AddConventionalJsonFileFinder(this IConventionalConfigurationBuilder builder)
        {
            builder.AddConventionalFileFinder(new JsonConfigurationFileFinder(builder.FileFinder));
            return builder;
        }
    }
}
