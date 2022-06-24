using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public readonly static SubjectEvent BeforeEveryRun = new SubjectEvent();

        /// <summary>
        /// The event is called when a next run starts.
        /// </summary>
        public readonly static SubjectEvent BeforeNextRun = new SubjectEvent();
    }
}
