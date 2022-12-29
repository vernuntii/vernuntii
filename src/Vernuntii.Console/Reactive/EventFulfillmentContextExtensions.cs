namespace Vernuntii.Reactive;

internal static class EventFulfillmentContextExtensions
{
    public static void ScheduleFulfillment<T>(this EventFulfillmentContext fulfillmentContext, IEventObserver<T> eventObserver, T eventData) =>
        fulfillmentContext.ScheduleEventInvocation(
            static result => {
                var (eventHandler, eventData) = (ValueTuple<Func<T, ValueTask>, T>)result;
                return eventHandler(eventData);
            },
            ((Func<T, ValueTask>)eventObserver.OnFulfilled, eventData));
}
