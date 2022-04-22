using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration.Shadowing;

namespace Vernuntii.Configuration
{
    /// <summary>
    /// Probes for configuration file.
    /// </summary>
    public interface IConventionalFileFinder
    {
        /// <summary>
        /// Checks if file name is appropriable for this file finder.
        /// </summary>
        /// <param name="fileName"></param>
        bool IsProbeable(string fileName);

        /// <summary>
        /// Finds file.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="fileName"></param>
        IFileFindingEnumerator FindFile(string directoryPath, string fileName);

        /// <summary>
        /// Adds file to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="filePath"></param>
        /// <param name="configureProviderBuilder">Configures building process of <see cref="IShadowedConfigurationProvider"/>.</param>
        void AddFile(IConfigurationBuilder builder, string filePath, Action<IShadowedConfigurationProviderBuilderConfigurator>? configureProviderBuilder = null);
    }
}
