namespace Vernuntii.Reactive;

internal class EventFulfillmentContext
{
    public IReadOnlyCollection<(Func<object, ValueTask>, object)> ScheduledEventInvocations =>
        _scheduledEventInvocations;

    private readonly List<(Func<object, ValueTask>, object)> _scheduledEventInvocations = new();

    public void ScheduleEventInvocation(Func<object, ValueTask> eventHandler, object eventData) =>
        _scheduledEventInvocations.Add((eventHandler, eventData));
}
