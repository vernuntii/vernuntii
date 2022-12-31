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
    }
}
