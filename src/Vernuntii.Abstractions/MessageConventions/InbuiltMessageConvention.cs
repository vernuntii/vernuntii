namespace Vernuntii.MessageConventions
{
    /// <summary>
    /// The inbuilt presets.
    /// </summary>
    public enum InbuiltMessageConvention
    {
        /// <summary>
        /// No message is considered to increment anything.
        /// </summary>
        Manual,
        /// <summary>
        /// Each message is incrementing patch.
        /// </summary>
        Continuous,
        /// <summary>
        /// Increment depending on Conventional Commits message.
        /// </summary>
        ConventionalCommits,
        /// <summary>
        /// Default message conventions is <see cref="Continuous"/>.
        /// </summary>
        Default
    }
}
