namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Compares plugin registrations. Compares <see cref="IPlugin.Order"/>
    /// and if equal then <see cref="IPluginRegistration.PluginId"></see>.
    /// </summary>
    public class PluginRegistrationComparer : Comparer<IPluginRegistration>
    {
        /// <summary>
        /// Default instance of this type.
        /// </summary>
        public new readonly static PluginRegistrationComparer Default = new PluginRegistrationComparer();

        /// <inheritdoc/>
        public override int Compare(IPluginRegistration? x, IPluginRegistration? y)
        {
            if (ReferenceEquals(x, y)) {
                return 0;
            } else if (x is null) {
                return -1;
            } else if (y is null) {
                return 1;
            }

            var result = Nullable.Compare(x.Plugin.Order, y.Plugin.Order);

            if (result < 0) {
                return -1;
            } else if (result > 0) {
                return 1;
            } else {
                return x.PluginId.CompareTo(y.PluginId);
            }
        }
    }
}
