using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// The plugin registry.
    /// </summary>
    internal sealed class PluginRegistry : IPluginRegistry, IAsyncDisposable, IDisposable
    {
        public IEnumerable<IPluginRegistration> PluginRegistrations =>
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

        public bool TryGetPlugin<T>([MaybeNullWhen(false)] out T plugin) where T : IPlugin
        {
            if (_pluginRegistrations.TryGetValue(typeof(T), out var pluginRegistration)) {
                plugin = (T)pluginRegistration.Plugin;
                return true;
            }

            plugin = default;
            return false;
        }

        public void Dispose() =>
            _pluginProvider.Dispose();

        public ValueTask DisposeAsync() =>
            _pluginProvider.DisposeAsync();
    }
}
