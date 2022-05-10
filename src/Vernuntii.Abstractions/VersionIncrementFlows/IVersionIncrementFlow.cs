namespace Vernuntii.VersionIncrementFlows
{
    /// <summary>
    /// Describes how an increment of a version part may flow to another version.
    /// </summary>
    public interface IVersionIncrementFlow : IEquatable<IVersionIncrementFlow>
    {
        /// <summary>
        /// The flow condition.
        /// </summary>
        VersionIncrementFlowCondition Condition { get; }

        /// <summary>
        /// Describes the version increment flow for the major part of the version.
        /// </summary>
        VersionIncrementFlowMode MajorFlow { get; }

        /// <summary>
        /// Describes the version increment flow for the minor part of the version.
        /// </summary>
        VersionIncrementFlowMode MinorFlow { get; }
    }
}
