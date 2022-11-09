namespace Vernuntii.PluginSystem.Meta
{
    /// <summary>
    /// Attribute describes metadata to that plugin (and implemented plugin services) to which this
    /// attribute is applied.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class PluginAttribute : Attribute
    {
        /// <summary>
        /// Order of the plugin.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// These specified plugin service types that are implemented to that plugin to which this
        /// attribute is applied are going to benefit of the functionality of this attribute. If empty,
        /// then all implemented plugin service types will benefit.
        /// </summary>
        public Type[] PluginServiceSelectors {
            get => _pluginServiceSelectors;
            init => _pluginServiceSelectors = value ?? throw new ArgumentNullException(nameof(value));
        }

        private Type[] _pluginServiceSelectors = Array.Empty<Type>();
    }
}
