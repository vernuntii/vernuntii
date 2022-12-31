namespace Vernuntii.Reactive;

internal class EventFulfillmentContext
{
    public IReadOnlyCollection<(Func<object, Task>, object)> ScheduledEventFulfillments =>
        _scheduledEventFulfillments;

    private readonly List<(Func<object, Task>, object)> _scheduledEventFulfillments = new();

    public void ScheduleEventFulfillment(Func<object, Task> eventHandler, object eventData) =>
        _scheduledEventFulfillments.Add((eventHandler, eventData));
}
