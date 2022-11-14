using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.PluginSystem
{
    internal class PluginDescriptorOrderComparer : Comparer<PluginDescriptor>
    {
        public new static readonly PluginDescriptorOrderComparer Default = new();

        /// <inheritdoc/>
        public override int Compare(PluginDescriptor? x, PluginDescriptor? y)
        {
            if (x is null || y is null) {
                throw new NotSupportedException("Nullable plugin descriptors are not supported");
            }

            if (x.PluginOrder < y.PluginOrder) {
                return -1;
            }

            return 1;

            //var result = Comparer<int?>.Default.Compare(x?.PluginOrder, y?.PluginOrder);
            //return result == 0 ? -1 : result;
        }
    }
}
