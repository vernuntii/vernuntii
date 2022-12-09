﻿namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Has responsibility to aggregate plugin events.
    /// </summary>
    public class PluginEventCache : IPluginEventCache
    {
        private readonly EventCache _eventAggregator = new();

        /// <inheritdoc/>
        public T GetEvent<T>(T uniqueEvent)
            where T : IEventFactory =>
            _eventAggregator.GetEvent(uniqueEvent);
    }
}
