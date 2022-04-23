using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Coordinates plugin execution.
    /// </summary>
    public class PluginExecutor
    {
        private PluginRegistry _pluginRegistry;
        private readonly IPluginEventAggregator _eventAggregator;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        /// <param name="eventAggregator"></param>
        public PluginExecutor(PluginRegistry pluginRegistry, IPluginEventAggregator eventAggregator)
        {
            _pluginRegistry = pluginRegistry;
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// Executes the plugins one after the other.
        /// </summary>
        public void Execute()
        {
            _pluginRegistry.Seal();

            foreach (var registration in _pluginRegistry.PluginRegistrations) {
                registration.Plugin.OnCompletedRegistration();
            }

            foreach (var registration in _pluginRegistry.PluginRegistrations) {
                registration.Plugin.OnEventAggregation(_eventAggregator);
            }
        }

        /// <summary>
        /// Destroys the plugins one after the other.
        /// </summary>
        public void Destroy()
        {
            foreach (var registration in _pluginRegistry.PluginRegistrations) {
                registration.Plugin.OnDestroy();
            }
        }
    }
}
