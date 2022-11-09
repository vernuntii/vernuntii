using Vernuntii.PluginSystem.Lifecycle;

namespace Vernuntii.PluginSystem
{
    internal record class PluginRegistration : IPluginRegistration
    {
        public int PluginId { get; }
        public bool Succeeded => PluginId >= 0;
        public Type ImplementationType => _pluginDescriptor.PluginType;
        public Type ServiceType => _pluginDescriptor.ServiceType;
        public IPlugin Plugin => _pluginDescriptor.Plugin;

        public bool ImplementsRegistrationAspect { get; }
        public bool ImplementsDestructionAspect { get; }

        private PluginDescriptorBase<IPlugin> _pluginDescriptor;

        public PluginRegistration(int pluginId, PluginDescriptorBase<IPlugin> pluginDescriptor)
        {
            PluginId = pluginId;
            _pluginDescriptor = pluginDescriptor;
            ImplementsRegistrationAspect = pluginDescriptor.Plugin is IPluginRegistrationAspect;
            ImplementsDestructionAspect = pluginDescriptor.Plugin is IPluginDestructionAspect;
        }
    }
}
