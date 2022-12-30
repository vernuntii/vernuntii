namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// The options for <see cref="VersionHashFile"/>.
    /// </summary>
    public class VersionHashFileOptions
    {
        /// <summary>
        /// The path to the .git-directory.
        /// </summary>
        public string GitDirectory { get; }

        /// <summary>
        /// The main configuration file.
        /// </summary>
        public string? ConfigFile { get; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="gitDirectory"></param>
        /// <param name="configFile"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public VersionHashFileOptions(string gitDirectory, string? configFile)
        {
            GitDirectory = gitDirectory ?? throw new ArgumentNullException(nameof(gitDirectory));
            ConfigFile = configFile;
        }
    }
}
