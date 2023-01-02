using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.PluginSystem
{
    internal class LateBoundPluginRegistry : IPluginRegistry
    {
        public IEnumerable<IPluginRegistration> PluginRegistrations =>
            GetPluginRegistry().PluginRegistrations;

        private IPluginRegistry? _pluginRegistry;

        public LateBoundPluginRegistry(out Action<IPluginRegistry> bindPluginRegistry) =>
            bindPluginRegistry = pluginRegistry => _pluginRegistry = pluginRegistry;

        private IPluginRegistry GetPluginRegistry() =>
            _pluginRegistry ?? throw new InvalidOperationException("Plugins have not been initialized yet");

        public T GetPlugin<T>() where T : IPlugin =>
            GetPluginRegistry().GetPlugin<T>();

        public bool TryGetPlugin<T>([MaybeNullWhen(false)] out T plugin) where T : IPlugin =>
            GetPluginRegistry().TryGetPlugin(out plugin);
    }
}
