using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.PluginSystem.Meta
{
    /// <summary>
    /// Attribute to announce that the plugin (and implemented plugin services) to which this attribute
    /// is applied depends on another plugin implementation.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ImportPluginAttribute<TService, TImplementation> : Attribute, IPluginDependencyDescriptor, IEquatable<ImportPluginAttribute<TService, TImplementation>>
        where TService : IPlugin
        where TImplementation : class, TService
    {
        /// <summary>
        /// If true, plugin of type <typeparamref name="TService"/> is tried to be registered.
        /// </summary>
        public bool TryRegister { get; init; }

        /// <summary>
        /// The plugin type.
        /// </summary>
        public Type ServiceType => _serviceType ??= typeof(TService);

        /// <summary>
        /// The plugin type.
        /// </summary>
        public Type ImplementationType => _implementationType ??= typeof(TImplementation);

        private Type? _serviceType;
        private Type? _implementationType;
        //private Type[] _pluginServiceSelectors = Array.Empty<Type>();

        /// <summary>
        /// The type that is used in registration.
        /// </summary>
        public ImportPluginAttribute() { }

        /// <inheritdoc/>
        public bool Equals([NotNullWhen(true)] IPluginDependencyDescriptor? other) =>
            other is not null
            && TryRegister == other.TryRegister
            && ServiceType == other.ServiceType
            && ImplementationType == other.ImplementationType;

        /// <inheritdoc/>
        public bool Equals([NotNullWhen(true)] ImportPluginAttribute<TService, TImplementation>? other) =>
            other is not null
            && Equals((IPluginDependencyDescriptor)this);

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? other) =>
            other is ImportPluginAttribute<TService, TImplementation> typedOther && Equals(typedOther)
            || (other is IPluginDependencyDescriptor typedOtherInterface && Equals(typedOtherInterface));

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(
            TryRegister,
            ServiceType,
            ImplementationType);
    }
}
