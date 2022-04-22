namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// The attribute describes a plugin.
    /// </summary>
    /// <typeparam name="T">The service type used in registration</typeparam>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class PluginAttribute<T> : Attribute
        where T : IPlugin
    {
        /// <summary>
        /// The type that is used in registration.
        /// </summary>
        public PluginAttribute() { }
    }
}
