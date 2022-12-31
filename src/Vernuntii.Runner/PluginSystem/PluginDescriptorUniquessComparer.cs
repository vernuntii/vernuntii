using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.PluginSystem
{
    internal class PluginDescriptorUniquessComparer : IEqualityComparer<PluginDescriptor>
    {
        public static readonly PluginDescriptorUniquessComparer Default = new();

        public bool Equals(PluginDescriptor? x, PluginDescriptor? y) =>
            ReferenceEquals(x, y)
            || (x is not null && y is not null
                && x.ServiceType == y.ServiceType
                && x.ImplementationType == y.ImplementationType);

        public int GetHashCode([DisallowNull] PluginDescriptor obj) => HashCode.Combine(
            obj.ServiceType,
            obj.ImplementationType);
    }
}
