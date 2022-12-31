namespace Vernuntii.Reactive;

internal static class EventFulfillmentContextExtensions
{
    public static void ScheduleFulfillment<T>(this EventFulfillmentContext fulfillmentContext, IEventFulfiller<T> eventObserver, T eventData) =>
        fulfillmentContext.ScheduleEventFulfillment(
            static result => {
                var (eventHandler, eventData) = (ValueTuple<Func<T, Task>, T>)result;
                return eventHandler(eventData);
            },
            ((Func<T, Task>)eventObserver.OnFulfilled, eventData));
}
