using Prism.Events;

namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Has responsibility to aggregate plugin events.
    /// </summary>
    public class PluginEventAggregator : IPluginEventAggregator
    {
        private EventAggregator _eventAggregator = new EventAggregator();

        /// <inheritdoc/>
        public TEventType GetEvent<TEventType>()
            where TEventType : EventBase, new() =>
            _eventAggregator.GetEvent<TEventType>();
    }
}
