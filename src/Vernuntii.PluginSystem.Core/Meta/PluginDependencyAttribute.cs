using Vernuntii.PluginSystem.Lifecycle;

namespace Vernuntii.PluginSystem.Meta
{
    /// <summary>
    /// Attribute to announce that the plugin (and implemented plugin services) to which this attribute
    /// is applied depends on another plugin implementation.
    /// </summary>
    /// <typeparam name="TPluginDependency">The plugin type of the dependency.</typeparam>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class PluginDependencyAttribute<TPluginDependency> : Attribute, IPluginDependencyDescriptor
        where TPluginDependency : class, IPlugin
    {
        /// <summary>
        /// If true, plugin of type <typeparamref name="TPluginDependency"/> is tried to be registered.
        /// </summary>
        public bool TryRegister { get; init; }

        /// <summary>
        /// The plugin type.
        /// </summary>
        public Type PluginDependency => _pluginDependencyType ??= typeof(TPluginDependency);

        /// <summary>
        /// These specified plugin service types that are implemented to that plugin to which this
        /// attribute is applied are going to benefit of the functionality of this attribute. If empty,
        /// then all implemented plugin service types will benefit.
        /// </summary>
        public Type[] PluginServiceSelectors {
            get => _pluginServiceSelectors;
            init => _pluginServiceSelectors = value ?? throw new ArgumentNullException(nameof(value));
        }

        private Type? _pluginDependencyType;
        private Type[] _pluginServiceSelectors = Array.Empty<Type>();

        /// <summary>
        /// The type that is used in registration.
        /// </summary>
        public PluginDependencyAttribute() { }
    }
}
