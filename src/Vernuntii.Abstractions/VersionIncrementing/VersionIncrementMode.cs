namespace Vernuntii.VersionIncrementing
{
    /// <summary>
    /// Version increment mode.
    /// </summary>
    public enum VersionIncrementMode
    {
        /// <summary>
        /// Does not increment anything.
        /// </summary>
        None = 0,
        /// <summary>
        /// Increment only the most significant version number once.
        /// </summary>
        Consecutive = 1,
        /// <summary>
        /// Increment most significant version number as often as indicated.
        /// </summary>
        Successive = 2
    }
}
