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
        /// <param name="uniqueEvent"></param>
        /// <param name="onNext"></param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public static IDisposable OnEveryEvent<TPayload>(this IEventCache eventCache, SubjectEvent<TPayload> uniqueEvent, Action<TPayload> onNext) =>
            eventCache.GetEvent(uniqueEvent).Subscribe(onNext);

        /// <summary>
        /// Subscribes an element handler to an observable sequence.
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="uniqueEvent"></param>
        /// <param name="onNext"></param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public static IDisposable OnEveryEvent(this IEventCache eventCache, SubjectEvent uniqueEvent, Action onNext) =>
            eventCache.GetEvent(uniqueEvent).Subscribe(_ => onNext());

        /// <summary>
        /// Subscribes an element handler to an observable sequence. Once called the element handler gets disposed and won't be called again.
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="eventCache"></param>
        /// <param name="uniqueEvent"></param>
        /// <param name="onNext"></param>
        /// <param name="onNextCondition"></param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public static IEventSubscription OnNextEvent<TPayload>(
            this IEventCache eventCache,
            SubjectEvent<TPayload> uniqueEvent,
            Action<TPayload> onNext,
            Func<bool>? onNextCondition = null)
        {
            var subscription = new EventSubscription();

            subscription.Disposable = eventCache
                .GetEvent(uniqueEvent)
                .Take(1)
                .Subscribe(payload => {
                    subscription.IncrementCounter();

                    if (onNextCondition?.Invoke() ?? true) {
                        onNext(payload);
                    }
                });

            return subscription;
        }

        /// <summary>
        /// Subscribes an element handler to an observable sequence. Once called the element handler gets disposed and won't be called again.
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="uniqueEvent"></param>
        /// <param name="onNext"></param>
        /// <param name="onNextCondition"></param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public static IEventSubscription OnNextEvent(
            this IEventCache eventCache,
            SubjectEvent uniqueEvent,
            Action onNext,
            Func<bool>? onNextCondition = null)
        {
            var subscription = new EventSubscription();

            subscription.Disposable = eventCache
                .GetEvent(uniqueEvent)
                .Take(1)
                .Subscribe(_ => {
                    subscription.IncrementCounter();

                    if (onNextCondition?.Invoke() ?? true) {
                        onNext();
                    }
                });

            return subscription;
        }

        /// <summary>
        /// Publishes an event.
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        public static void FireEvent<TPayload>(this IEventCache eventCache, SubjectEvent<TPayload> uniqueEvent, TPayload payload) =>
            eventCache.GetEvent(uniqueEvent).Publish(payload);

        /// <summary>
        /// Publishes an event.
        /// </summary>
        public static void FireEvent(this IEventCache eventCache, SubjectEvent uniqueEvent) =>
            eventCache.GetEvent(uniqueEvent).Publish();
    }
}
