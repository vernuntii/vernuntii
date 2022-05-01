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
        public readonly static SubjectEvent EnableLoggingInfrastructure = new SubjectEvent();

        /// <summary>
        /// Event when logging infrastructure is enabled.
        /// </summary>
        public readonly static SubjectEvent<ILoggingPlugin> EnabledLoggingInfrastructure = new SubjectEvent<ILoggingPlugin>();
    }
}
