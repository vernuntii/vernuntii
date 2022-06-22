using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Coordinates plugin execution.
    /// </summary>
    public class PluginExecutor
    {
        private PluginRegistry _pluginRegistry;
        private readonly IPluginEventCache _eventAggregator;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        /// <param name="eventAggregator"></param>
        public PluginExecutor(PluginRegistry pluginRegistry, IPluginEventCache eventAggregator)
        {
            _pluginRegistry = pluginRegistry;
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// Executes the plugins one after the other.
        /// </summary>
        public async ValueTask ExecuteAsync()
        {
            _pluginRegistry.Seal();

            foreach (var registration in _pluginRegistry.PluginRegistrations) {
                await registration.Plugin.OnAfterRegistration();
            }

            foreach (var registration in _pluginRegistry.PluginRegistrations) {
                await registration.Plugin.OnEvents(_eventAggregator);
            }
        }

        /// <summary>
        /// Destroys the plugins one after the other.
        /// </summary>
        public async ValueTask DestroyAsync()
        {
            foreach (var registration in _pluginRegistry.PluginRegistrations) {
                await registration.Plugin.OnDestroy();
            }
        }
    }
}
