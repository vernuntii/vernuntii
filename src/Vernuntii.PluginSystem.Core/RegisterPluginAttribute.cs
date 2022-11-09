using Vernuntii.PluginSystem.Lifecycle;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// If used it registers the annotated class as plugin.
    /// </summary>
    /// <typeparam name="T">The service type used in registration</typeparam>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class RegisterPluginAttribute<T> : Attribute
        where T : IPlugin
    {
        public int Equal { get; }

        /// <summary>
        /// The type that is used in registration.
        /// </summary>
        public RegisterPluginAttribute() { }
    }
}
