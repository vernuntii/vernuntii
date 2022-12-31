using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// The plugin registry.
    /// </summary>
    internal sealed class PluginRegistry : IPluginRegistry, IAsyncDisposable, IDisposable
    {
        public IEnumerable<IPluginRegistration> OrderlyPluginRegistrations =>
            _pluginRegistrations.Values;

        private readonly IReadOnlyDictionary<Type, IPluginRegistration> _pluginRegistrations;
        private readonly ServiceProvider _pluginProvider;

        public PluginRegistry(IReadOnlyDictionary<Type, IPluginRegistration> pluginRegistrations, ServiceProvider pluginProvider)
        {
            _pluginRegistrations = pluginRegistrations ?? throw new ArgumentNullException(nameof(pluginRegistrations));
            _pluginProvider = pluginProvider ?? throw new ArgumentNullException(nameof(pluginProvider));
        }

        public T GetPlugin<T>() where T : IPlugin =>
            (T)_pluginRegistrations[typeof(T)].Plugin;

        public void Dispose() =>
            _pluginProvider.Dispose();

        public ValueTask DisposeAsync() =>
            _pluginProvider.DisposeAsync();
    }
}
