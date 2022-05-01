namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Caches events.
    /// </summary>
    public interface IEventCache
    {
        /// <summary>
        /// Gets the event for <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns>Either existing or new event.</returns>
        TEvent GetEvent<TEvent>(TEvent eventTemplate)
            where TEvent : IEventFactory;
    }
}
