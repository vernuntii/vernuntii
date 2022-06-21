using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration.Shadowing;

namespace Vernuntii.Configuration.Yaml
{
    /// <summary>
    /// A file finder that can find .yml-files.
    /// </summary>
    public class YamlConfigurationFileFinder : ConventionalFileFinderBase
    {
        private readonly static string[] DefaultFileNames = new[] {
            YamlConfigurationFileDefaults.YmlFileName,
            YamlConfigurationFileDefaults.YamlFileName
        };

        /// <inheritdoc/>
        protected override string[] ProbeableFileExtensions { get; } = new[] {
            YamlConfigurationFileDefaults.YmlFileExtension,
            YamlConfigurationFileDefaults.YamlFileExtension
        };

        private readonly IFileFinder _fileFinder;

        /// <summary>
        /// Instance constructor.
        /// </summary>
        /// <param name="fileFinder"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public YamlConfigurationFileFinder(IFileFinder fileFinder) =>
            _fileFinder = fileFinder ?? throw new ArgumentNullException(nameof(fileFinder));

        /// <inheritdoc/>
        public override IFileFindingEnumerator FindFile(string directoryPath, string? fileName = null)
        {
            if (string.IsNullOrEmpty(fileName)) {
                return _fileFinder.FindFile(directoryPath, DefaultFileNames);
            }

            return _fileFinder.FindFile(directoryPath, fileName);
        }

        /// <inheritdoc/>
        public override void AddFile(IConfigurationBuilder builder, string filePath, Action<IShadowedConfigurationProviderBuilderConfigurator>? configureProviderBuilder = null)
        {
            var configurationSource = new YamlShadowedConfigurationSource() {
                Path = filePath,
                Optional = false,
                OnBuildShadowedConfigurationProvider = configureProviderBuilder,
            };

            configurationSource.ResolveFileProvider();
            builder.Add(configurationSource);
        }
    }
}
