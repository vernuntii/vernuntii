namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Events for <see cref="ILoggingPlugin"/>.
    /// </summary>
    public static class LoggingEvents
    {
        /// <summary>
        /// Event when logging infrastructure is getting enabled.
        /// </summary>
        public sealed class EnableLoggingInfrastructureEvent : PubSubEvent
        {
        }

        /// <summary>
        /// Event when logging infrastructure is enabled.
        /// </summary>
        public sealed class EnabledLoggingInfrastructureEvent : PubSubEvent<EnabledLoggingInfrastructureEvent, ILoggingPlugin>
        {
        }
    }
}
