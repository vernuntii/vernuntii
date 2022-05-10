namespace Vernuntii.VersionIncrementFlows
{
    /// <summary>
    /// The condition when flowing is allowed.
    /// </summary>
    public enum VersionIncrementFlowCondition
    {
        /// <summary>
        /// A condition that does not lead into a flow of any version part at any circumstances.
        /// </summary>
        Never = 0,
        /// <summary>
        /// Allow flow when major is zero.
        /// </summary>
        ZeroMajor = 1
    }
}
