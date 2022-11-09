using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vernuntii.PluginSystem.Lifecycle;
using Vernuntii.PluginSystem.Meta;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// The base plugin descriptor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public record PluginDescriptorBase<T>
        where T : IPlugin?
    {

        /// <summary>
        /// The service type the plugin is associated with.
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// The instantiated plugin.
        /// </summary>
        public T Plugin { get; internal init; }

        /// <summary>
        /// The plugin type.
        /// </summary>
        public Type PluginType { get; }

        /// <summary>
        /// Describes the plugins this plugin depends on.
        /// </summary>
        public IReadOnlyList<IPluginDependencyDescriptor> PluginDependencies { get; internal init; }

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="pluginType"></param>
        /// <param name="serviceType"></param>
        internal PluginDescriptorBase(Type serviceType, T plugin, Type pluginType)
        {
            Plugin = plugin;
            PluginType = pluginType;
            ServiceType = serviceType;
            PluginDependencies = Array.Empty<IPluginDependencyDescriptor>();
        }
    }
}
