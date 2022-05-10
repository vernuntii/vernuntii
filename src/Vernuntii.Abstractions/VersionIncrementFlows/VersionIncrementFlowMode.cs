namespace Vernuntii.VersionIncrementFlows
{
    /// <summary>
    /// Describes how the version part should flow.
    /// </summary>
    public enum VersionIncrementFlowMode
    {
        /// <summary>
        /// Version part won't flow.
        /// </summary>
        None = 0,
        /// <summary>
        /// Version part will flow to next version part.
        /// </summary>
        Downstream = 1,
    }
}
