using Vernuntii.PluginSystem.Events;
using Vernuntii.PluginSystem.Lifecycle;

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
        public PluginExecutor(PluginRegistry pluginRegistry, IPluginEventCache pluginEvents)
        {
            _pluginRegistry = pluginRegistry;
            _pluginEvents = pluginEvents;
        }

        /// <summary>
        /// Executes the plugins one after the other.
        /// </summary>
        public async ValueTask ExecuteAsync()
        {
            await _pluginRegistry.CompleteRegistrationPhase();

            foreach (var registration in _pluginRegistry.PluginRegistrations) {
                await registration.Plugin.OnExecution(_pluginEvents);
            }
        }

        /// <summary>
        /// Destroys the plugins one after the other.
        /// </summary>
        public async ValueTask DestroyAsync()
        {
            foreach (var registration in _pluginRegistry.PluginRegistrations) {
                if (!registration.ImplementsDestructionAspect) {
                    continue;
                }

                await ((IPluginDestructionAspect)registration.Plugin).OnDestroy();
            }
        }
    }
}
