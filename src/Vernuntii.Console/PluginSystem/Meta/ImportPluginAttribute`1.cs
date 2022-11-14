using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.PluginSystem.Meta
{
    /// <summary>
    /// Attribute to announce that the plugin (and implemented plugin services) to which this attribute
    /// is applied depends on another plugin implementation.
    /// </summary>
    /// <typeparam name="TPlugin">The plugin type of the dependency.</typeparam>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ImportPluginAttribute<TPlugin> : ImportPluginAttribute<TPlugin, TPlugin>
        where TPlugin : class, IPlugin
    {
    }
}
