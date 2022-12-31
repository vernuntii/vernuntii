using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Coordinates plugin execution.
    /// </summary>
    public class PluginExecutor
    {
        private readonly PluginRegistry _pluginRegistry;
        private readonly EventSystem _pluginEvents;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        /// <param name="pluginEvents"></param>
        internal PluginExecutor(PluginRegistry pluginRegistry, EventSystem pluginEvents)
        {
            _pluginRegistry = pluginRegistry;
            _pluginEvents = pluginEvents;
        }

        /// <summary>
        /// Executes the plugins one after the other.
        /// </summary>
        public async Task ExecuteAsync()
        {
            foreach (var pluginRegistration in _pluginRegistry.OrderlyPluginRegistrations) {
                var task = pluginRegistration.Plugin.OnExecution(_pluginEvents);

                if (!task.IsCompletedSuccessfully) {
                    await task.ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Destroys the plugins.
        /// </summary>
        public ValueTask DestroyAsync() =>
            _pluginRegistry.DisposeAsync();
    }
}
