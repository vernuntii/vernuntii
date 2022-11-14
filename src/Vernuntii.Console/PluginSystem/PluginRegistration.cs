using Vernuntii.PluginSystem;

namespace Vernuntii.PluginSystem
{
    internal record class PluginRegistration : IPluginRegistration
    {
        public int PluginId { get; }
        public bool Succeeded => PluginId >= 0;
        public Type ImplementationType => _pluginDescriptor.ImplementationType;
        public Type ServiceType => _pluginDescriptor.ServiceType;
        public IPlugin Plugin { get; }

        private PluginDescriptor _pluginDescriptor;

        public PluginRegistration(IPlugin plugin, int pluginId, PluginDescriptor pluginDescriptor)
        {
            Plugin = plugin;
            PluginId = pluginId;
            _pluginDescriptor = pluginDescriptor;
        }
    }
}
