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
        public static IDisposable Subscribe<TPayload>(this IEventCache eventCache, SubjectEvent<TPayload> eventTemplate, Action<TPayload> onNext) =>
            eventCache.GetEvent(eventTemplate).Subscribe(onNext);

        /// <summary>
        /// Subscribes an element handler to an observable sequence.
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="eventTemplate"></param>
        /// <param name="onNext"></param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public static IDisposable Subscribe(this IEventCache eventCache, SubjectEvent eventTemplate, Action onNext) =>
            eventCache.GetEvent(eventTemplate).Subscribe(_ => onNext());

        /// <summary>
        /// Subscribes an element handler to an observable sequence. Once called the element handler gets disposed and won't be called again.
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="eventCache"></param>
        /// <param name="eventTemplate"></param>
        /// <param name="onNext"></param>
        /// <param name="onNextCondition"></param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public static IOneSignalSubscription SubscribeOnce<TPayload>(
            this IEventCache eventCache,
            SubjectEvent<TPayload> eventTemplate,
            Action<TPayload> onNext,
            Func<bool>? onNextCondition = null)
        {
            var subscription = new OneSignalSubscription();

            subscription.Disposable = eventCache
                .GetEvent(eventTemplate)
                .Take(1)
                .Subscribe(payload => {
                    subscription.SignaledOnce = true;

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
        /// <param name="eventTemplate"></param>
        /// <param name="onNext"></param>
        /// <param name="onNextCondition"></param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public static IOneSignalSubscription SubscribeOnce(
            this IEventCache eventCache,
            SubjectEvent eventTemplate,
            Action onNext,
            Func<bool>? onNextCondition = null)
        {
            var subscription = new OneSignalSubscription();

            subscription.Disposable = eventCache
                .GetEvent(eventTemplate)
                .Take(1)
                .Subscribe(_ => {
                    subscription.SignaledOnce = true;

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
        public static void Publish<TPayload>(this IEventCache eventCache, SubjectEvent<TPayload> eventTemplate, TPayload payload) =>
            eventCache.GetEvent(eventTemplate).Publish(payload);

        /// <summary>
        /// Publishes an event.
        /// </summary>
        public static void Publish(this IEventCache eventCache, SubjectEvent eventTemplate) =>
            eventCache.GetEvent(eventTemplate).Publish();
    }
}
