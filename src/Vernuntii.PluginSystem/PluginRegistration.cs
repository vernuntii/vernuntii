namespace Vernuntii.PluginSystem
{
    internal record class PluginRegistration : IPluginRegistration
    {
        public int PluginId { get; }
        public Type ServiceType { get; }
        public IPlugin Plugin { get; }
        public bool Succeeded => PluginId >= 0;

        public PluginRegistration(int pluginId, Type serviceType, IPlugin plugin)
        {
            PluginId = pluginId;
            Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        }
    }
}
