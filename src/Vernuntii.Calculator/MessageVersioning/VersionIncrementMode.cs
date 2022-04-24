namespace Vernuntii.MessageVersioning
{
    /// <summary>
    /// Version increment mode.
    /// </summary>
    public enum VersionIncrementMode
    {
        /// <summary>
        /// Does not increment anything.
        /// </summary>
        None,
        /// <summary>
        /// Increment only the most significant version number once.
        /// </summary>
        Consecutive,
        /// <summary>
        /// Increment most significant version number as often as indicated.
        /// </summary>
        Successive
    }
}
