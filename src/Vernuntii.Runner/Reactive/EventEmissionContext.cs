namespace Vernuntii.Reactive;

internal sealed record EventEmissionContext
{
    /// <summary>
    /// Indicates whether this emission represents the last event.
    /// </summary>
    public bool IsCompleting { get; init; }

    public IReadOnlyCollection<(Func<object, Task>, object)> ScheduledEventEmissions =>
        _scheduledEventEmissions;

    private readonly List<(Func<object, Task>, object)> _scheduledEventEmissions = new();

    public void ScheduleEventEmission(Func<object, Task> eventEmitter, object eventData) =>
        _scheduledEventEmissions.Add((eventEmitter, eventData));
}
