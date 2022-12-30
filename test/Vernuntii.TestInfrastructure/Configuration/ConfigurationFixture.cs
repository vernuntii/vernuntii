using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration.Json;
using Vernuntii.Configuration.Yaml;
using Vernuntii.IO;

namespace Vernuntii.Configuration
{
    public class ConfigurationFixture
    {
        public static readonly ConfigurationFixture Default = new();

        private static IConfiguration FindConfigurationFile(IConventionalFileFinder fileFinder, string directoryPath, string fileName) =>
            new ConventionalConfigurationBuilder()
                .AddConventionalFileFinder(fileFinder)
                .AddFirstConventionalFile(directoryPath, new[] { fileName }, out _)
                .Build();

        public IFileFinder FileFinder { get; }
        public YamlConfigurationFileFinder YamlConfigurationFileFinder { get; }
        public JsonConfigurationFileFinder JsonConfigurationFileFinder { get; }

        public ConfigurationFixture()
        {
            FileFinder = new FileFinder();
            YamlConfigurationFileFinder = new YamlConfigurationFileFinder(FileFinder);
            JsonConfigurationFileFinder = new JsonConfigurationFileFinder(FileFinder);
        }

        public IConfiguration FindYamlConfigurationFile(string directoryPath, string fileName) =>
            FindConfigurationFile(YamlConfigurationFileFinder, directoryPath, fileName);

        public IConfiguration FindJsonConfigurationFile(string directoryPath, string fileName) =>
            FindConfigurationFile(JsonConfigurationFileFinder, directoryPath, fileName);
    }
}
