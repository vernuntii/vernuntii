﻿using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Caches events.
    /// </summary>
    public class EventCache : IEventCache
    {
        private readonly Dictionary<object, object> _eventDictionary = new();
        private readonly object _lockObject = new();

        private bool TryGetEvent<T>(object key, [NotNullWhen(true)] out T? typedEvent)
        {
            if (_eventDictionary.TryGetValue(key, out var eventObject)) {
                typedEvent = (T)eventObject;
                return true;
            }

            typedEvent = default;
            return false;
        }

        /// <inheritdoc/>
        public TEvent GetEvent<TEvent>(TEvent uniqueEvent)
            where TEvent : IEventFactory
        {
            {
                // Choose fastest way without locking and
                // when first failing then initialize event.
                if (TryGetEvent<TEvent>(uniqueEvent, out var typedEvent)) {
                    return typedEvent;
                }
            }

            lock (_lockObject) {
                if (TryGetEvent<TEvent>(uniqueEvent, out var typedEvent)) {
                    return typedEvent;
                }

                typedEvent = (TEvent)uniqueEvent.CreateEvent();
                _eventDictionary.Add(uniqueEvent, typedEvent);
                return typedEvent;
            }
        }
    }
}
