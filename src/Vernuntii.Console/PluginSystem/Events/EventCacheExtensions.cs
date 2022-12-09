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
        /// <param name="uniqueKey"></param>
        /// <param name="onNext"></param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public static IDisposable Subscribe<TPayload>(this IEventCache eventCache, SubjectEvent<TPayload> uniqueKey, Action<TPayload> onNext) =>
            eventCache.GetEvent(uniqueKey).Subscribe(onNext);

        /// <summary>
        /// Subscribes an element handler to an observable sequence.
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="onNext"></param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public static IDisposable Subscribe(this IEventCache eventCache, SubjectEvent uniqueKey, Action onNext) =>
            eventCache.GetEvent(uniqueKey).Subscribe(_ => onNext());

        /// <summary>
        /// Subscribes an element handler to an observable sequence. Once called the element handler gets disposed and won't be called again.
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="eventCache"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="onNext"></param>
        /// <param name="onNextCondition"></param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public static IEventSubscription SubscribeOnce<TPayload>(
            this IEventCache eventCache,
            SubjectEvent<TPayload> uniqueKey,
            Action<TPayload> onNext,
            Func<bool>? onNextCondition = null)
        {
            var subscription = new EventSubscription();

            subscription.Disposable = eventCache
                .GetEvent(uniqueKey)
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
        /// <param name="uniqueKey"></param>
        /// <param name="onNext"></param>
        /// <param name="onNextCondition"></param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public static IEventSubscription SubscribeOnce(
            this IEventCache eventCache,
            SubjectEvent uniqueKey,
            Action onNext,
            Func<bool>? onNextCondition = null)
        {
            var subscription = new EventSubscription();

            subscription.Disposable = eventCache
                .GetEvent(uniqueKey)
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
        public static void Publish<TPayload>(this IEventCache eventCache, SubjectEvent<TPayload> uniqueEvent, TPayload payload) =>
            eventCache.GetEvent(uniqueEvent).Publish(payload);

        /// <summary>
        /// Publishes an event.
        /// </summary>
        public static void Publish(this IEventCache eventCache, SubjectEvent uniqueEvent) =>
            eventCache.GetEvent(uniqueEvent).Publish();
    }
}
