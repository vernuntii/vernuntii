namespace Vernuntii.VersionIncrementFlows
{
    /// <summary>
    /// Inbuilt version increment flows.
    /// </summary>
    public enum InbuiltVersionIncrementFlow
    {
        /// <summary>
        /// Does never flow.
        /// </summary>
        None,
        /// <summary>
        /// Does flow every version part downstream when major is zero.
        /// </summary>
        ZeroMajorDownstream
    }
}
