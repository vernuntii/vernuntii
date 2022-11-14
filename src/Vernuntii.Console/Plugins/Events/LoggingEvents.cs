using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="ILoggingPlugin"/>.
    /// </summary>
    public static class LoggingEvents
    {
        /// <summary>
        /// Event when logging infrastructure is getting enabled.
        /// </summary>
        public static readonly SubjectEvent EnableLoggingInfrastructure = new();

        /// <summary>
        /// Event when logging infrastructure is enabled.
        /// </summary>
        public static readonly SubjectEvent<ILoggingPlugin> EnabledLoggingInfrastructure = new();
    }
}
