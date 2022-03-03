using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration.Shadowing;

namespace Vernuntii.Configuration.Json
{
    /// <summary>
    /// A file finder that can find .json-files.
    /// </summary>
    public class JsonConfigurationFileFinder : ConventionalFileFinderBase
    {
        private readonly IFileFinder _fileFinder;

        /// <inheritdoc/>
        protected override string[] ProbeableFileExtensions { get; } = new[] { JsonConfigurationFileDefaults.JsonFileExtension };

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="fileFinder"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public JsonConfigurationFileFinder(IFileFinder fileFinder) =>
            _fileFinder = fileFinder ?? throw new ArgumentNullException(nameof(fileFinder));

        /// <inheritdoc/>
        public override IFileFindingEnumerator FindFile(string directoryPath, string? fileName = null)
        {
            if (string.IsNullOrEmpty(fileName)) {
                fileName = JsonConfigurationFileDefaults.JsonFileName;
            }

            return _fileFinder.FindFile(directoryPath, fileName);
        }

        /// <inheritdoc/>
        public override void AddFile(IConfigurationBuilder builder, string filePath, Action<IShadowedConfigurationProviderBuilderConfigurator>? configureProviderBuilder = null)
        {
            var configurationSource = new JsonShadowedConfigurationSource() {
                Path = filePath,
                Optional = false,
                OnBuildShadowedConfigurationProvider = configureProviderBuilder,
            };

            configurationSource.ResolveFileProvider();
            builder.Add(configurationSource);
            builder.AddJsonFile(filePath);
        }
    }
}
