namespace Vernuntii.VersionIncrementFlows
{
    /// <summary>
    /// Describes how to flow each part of a version.
    /// </summary>
    public record class VersionIncrementFlow : IVersionIncrementFlow
    {
        /// <summary>
        /// An instance where each version part does not flow.
        /// </summary>
        public readonly static VersionIncrementFlow None = new VersionIncrementFlow();

        /// <summary>
        /// An instance where every version part flows downstream when major is zero.
        /// </summary>
        public readonly static VersionIncrementFlow ZeroMajorDownstream = new VersionIncrementFlow() {
            Condition = VersionIncrementFlowCondition.ZeroMajor,
            MajorFlow = VersionIncrementFlowMode.Downstream,
            MinorFlow = VersionIncrementFlowMode.Downstream
        };

        /// <inheritdoc/>
        public VersionIncrementFlowCondition Condition { get; init; }

        /// <inheritdoc/>
        public VersionIncrementFlowMode MajorFlow { get; init; }

        /// <inheritdoc/>
        public VersionIncrementFlowMode MinorFlow { get; init; }

        /// <inheritdoc/>
        public VersionIncrementFlowMode PatchFlow { get; init; }

        /// <inheritdoc/>
        public virtual bool Equals(VersionIncrementFlow? other) =>
            VersionIncrementFlowEqualityComparer.Default.Equals(this, other);

        bool IEquatable<IVersionIncrementFlow>.Equals(IVersionIncrementFlow? other) =>
            VersionIncrementFlowEqualityComparer.Default.Equals(this, other);

        /// <inheritdoc/>
        public override int GetHashCode() =>
            VersionIncrementFlowEqualityComparer.Default.GetHashCode(this);
    }
}
