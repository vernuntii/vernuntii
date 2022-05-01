namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Has responsibility to aggregate plugin events.
    /// </summary>
    public class PluginEventCache : IPluginEventCache
    {
        private EventCache _eventAggregator = new EventCache();

        /// <inheritdoc/>
        public T GetEvent<T>(T eventTemplate)
            where T : IEventFactory =>
            _eventAggregator.GetEvent(eventTemplate);
    }
}
