namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Can create an event.
    /// </summary>
    public interface IEventFactory
    {
        /// <summary>
        /// Creates an event.
        /// </summary>
        object CreateEvent();
    }
}
