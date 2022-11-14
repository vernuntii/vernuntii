using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Coordinates plugin execution.
    /// </summary>
    public class PluginExecutor
    {
        private PluginRegistry _pluginRegistry;
        private readonly IPluginEventCache _pluginEvents;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        /// <param name="pluginEvents"></param>
        internal PluginExecutor(PluginRegistry pluginRegistry, IPluginEventCache pluginEvents)
        {
            _pluginRegistry = pluginRegistry;
            _pluginEvents = pluginEvents;
        }

        /// <summary>
        /// Executes the plugins one after the other.
        /// </summary>
        public async ValueTask ExecuteAsync()
        {
            foreach (var pluginRegistration in _pluginRegistry.OrderlyPluginRegistrations) {
                await pluginRegistration.Plugin.OnExecution(_pluginEvents);
            }
        }

        /// <summary>
        /// Destroys the plugins.
        /// </summary>
        public ValueTask DestroyAsync() =>
            _pluginRegistry.DisposeAsync();
    }
}
