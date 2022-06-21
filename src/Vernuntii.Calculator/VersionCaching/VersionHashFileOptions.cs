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
        public string DotGitDirectory { get; }

        /// <summary>
        /// The main configuration file.
        /// </summary>
        public string? ConfigFile { get; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="dotGitDirectory"></param>
        /// <param name="configFile"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public VersionHashFileOptions(string dotGitDirectory, string? configFile)
        {
            DotGitDirectory = dotGitDirectory ?? throw new ArgumentNullException(nameof(dotGitDirectory));
            ConfigFile = configFile;
        }
    }
}
