using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events not for a plugin but for the lifecycle.
    /// </summary>
    public static class LifecycleEvents
    {
        /// <summary>
        /// The event is called whenever a run starts.
        /// </summary>
        public static readonly SubjectEvent BeforeEveryRun = new();

        /// <summary>
        /// The event is called when a next run starts.
        /// </summary>
        public static readonly SubjectEvent BeforeNextRun = new();
    }
}
