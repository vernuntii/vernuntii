namespace Vernuntii.MessageVersioning
{
    /// <summary>
    /// The inbuilt presets.
    /// </summary>
    public enum VersioningModeMessageConvention
    {
        /// <summary>
        /// No message is considered to increment anything.
        /// </summary>
        Manual,
        /// <summary>
        /// Each message is incrementing patch.
        /// </summary>
        Continous,
        /// <summary>
        /// Increment depending on Conventional Commits message.
        /// </summary>
        ConventionalCommits
    }
}
