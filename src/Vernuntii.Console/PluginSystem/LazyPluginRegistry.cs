namespace Vernuntii.PluginSystem
{
    internal class LazyPluginRegistry : IPluginRegistry
    {
        public IEnumerable<IPluginRegistration> OrderlyPluginRegistrations =>
            GetPluginRegistry().OrderlyPluginRegistrations;

        private IPluginRegistry? _pluginRegistry;

        public LazyPluginRegistry(out Action<IPluginRegistry> commitPluginRegistry) =>
            commitPluginRegistry = pluginRegistry => _pluginRegistry = pluginRegistry;

        private IPluginRegistry GetPluginRegistry() =>
            _pluginRegistry ?? throw new InvalidOperationException("Plugins have not been initialized yet");

        public T GetPlugin<T>() where T : IPlugin =>
            GetPluginRegistry().GetPlugin<T>();
    }
}
