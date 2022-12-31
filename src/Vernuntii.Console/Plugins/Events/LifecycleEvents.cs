using Vernuntii.Plugins.Lifecycle;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events not for a plugin but for the lifecycle.
    /// </summary>
    public static class LifecycleEvents
    {
        /// <summary>
        /// The event is called before a run is started.
        /// </summary>
        public static readonly EventDiscriminator<LifecycleContext> BeforeEveryRun = EventDiscriminator.New<LifecycleContext>();

        /// <summary>
        /// The event is called before the next run starts.
        /// </summary>
        public static readonly EventDiscriminator<LifecycleContext> BeforeNextRun = EventDiscriminator.New<LifecycleContext>();
    }
}
