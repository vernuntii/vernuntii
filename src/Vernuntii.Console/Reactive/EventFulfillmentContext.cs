namespace Vernuntii.Reactive;

internal class EventFulfillmentContext
{
    public IReadOnlyCollection<(Func<object, Task>, object)> ScheduledEventInvocations =>
        _scheduledEventInvocations;

    private readonly List<(Func<object, Task>, object)> _scheduledEventInvocations = new();

    public void ScheduleEventInvocation(Func<object, Task> eventHandler, object eventData) =>
        _scheduledEventInvocations.Add((eventHandler, eventData));
}
