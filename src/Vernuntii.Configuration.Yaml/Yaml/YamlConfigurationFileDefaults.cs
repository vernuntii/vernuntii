namespace Vernuntii.Configuration.Yaml
{
    /// <summary>
    /// Defaults of configuration file.
    /// </summary>
    public static class YamlConfigurationFileDefaults
    {
        /// <summary>
        /// ".yml"
        /// </summary>
        public const string YmlFileExtension = ".yml";

        /// <summary>
        /// ".yml"
        /// </summary>
        public const string YamlFileExtension = ".yaml";

        /// <summary>
        /// The .yml configuration file name.
        /// </summary>
        public const string YmlFileName = ConfigurationFileDefaults.DefaultFileNameWithoutExtension + YmlFileExtension;

        /// <summary>
        /// The .yaml configuration file name.
        /// </summary>
        public const string YamlFileName = ConfigurationFileDefaults.DefaultFileNameWithoutExtension + YamlFileExtension;
    }
}
