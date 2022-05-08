namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// The inbuilt message indicators.
    /// </summary>
    public enum InbuiltMessageIndicator
    {
        /// <summary>
        /// Always indicates false.
        /// </summary>
        Falsy,
        /// <summary>
        /// Always indicates true.
        /// </summary>
        Truthy,
        /// <summary>
        /// Message indicator depending on Conventional Commits.
        /// </summary>
        ConventionalCommits,
    }
}
