using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionIncrementFlows
{
    /// <inheritdoc/>
    public class VersionIncrementFlowEqualityComparer : EqualityComparer<IVersionIncrementFlow>
    {
        /// <summary>
        /// Default instance of this type.
        /// </summary>
        public static new readonly VersionIncrementFlowEqualityComparer Default = new();

        /// <inheritdoc/>
        public override bool Equals(IVersionIncrementFlow? x, IVersionIncrementFlow? y) =>
            ReferenceEquals(x, y)
            || (x is not null
                && y is not null
                && x.MajorFlow == y.MajorFlow
                && x.MajorFlow == y.MinorFlow);

        /// <inheritdoc/>
        public override int GetHashCode([DisallowNull] IVersionIncrementFlow obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.MajorFlow);
            hashCode.Add(obj.MinorFlow);
            return hashCode.ToHashCode();
        }
    }
}
