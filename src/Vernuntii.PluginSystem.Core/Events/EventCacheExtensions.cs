using System.Reactive.Linq;

namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Extension methods for <see cref="IPluginEventCache"/>.
    /// </summary>
    public static class EventCacheExtensions
    {
        /// <summary>
        /// Subscribes an element handler to an observable sequence.
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="eventCache"></param>
        /// <param name="eventTemplate"></param>
        /// <param name="onNext"></param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public static IDisposable SubscribeOnce<TPayload>(this IEventCache eventCache, SubjectEvent<TPayload> eventTemplate, Action<TPayload> onNext) =>
            eventCache.GetEvent(eventTemplate).Take(1).Subscribe(onNext);

        /// <summary>
        /// Subscribes an element handler to an observable sequence.
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="eventTemplate"></param>
        /// <param name="onNext"></param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public static IDisposable SubscribeOnce(this IEventCache eventCache, SubjectEvent eventTemplate, Action onNext) =>
            eventCache.GetEvent(eventTemplate).Take(1).Subscribe(_ => onNext());

        /// <summary>
        /// Publishes an event.
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        public static void Publish<TPayload>(this IEventCache eventCache, SubjectEvent<TPayload> eventTemplate, TPayload payload) =>
            eventCache.GetEvent(eventTemplate).Publish(payload);

        /// <summary>
        /// Publishes an event.
        /// </summary>
        public static void Publish(this IEventCache eventCache, SubjectEvent eventTemplate) =>
            eventCache.GetEvent(eventTemplate).Publish();
    }
}
