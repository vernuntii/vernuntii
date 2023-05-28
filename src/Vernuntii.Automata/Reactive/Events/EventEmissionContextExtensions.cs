namespace Vernuntii.Reactive.Events;

internal static class EventEmissionBacklogExtensions
{
    public static void ToEmissionBacklog<T>(this IEventObserver<T> eventObserver, EventEmissionBacklog emissionBacklog, T eventData) =>
        emissionBacklog.AddEventEmission(
            static result => {
                var (eventObserver, eventData) = (ValueTuple<Func<T, Task>, T>)result;
                return eventObserver(eventData);
            },
            ((Func<T, Task>)eventObserver.OnEmissionAsync, eventData));

    internal static void OnEmissionOrBacklog<T>(this IEventObserver<T> eventObserver, EventEmissionBacklog emissionBacklog, T eventData)
    {
        if (eventObserver.ContinueSynchronousEmissionChaining) {
            eventObserver.OnEmission(emissionBacklog, eventData);
            return;
        }

        eventObserver.ToEmissionBacklog(emissionBacklog, eventData);
    }
}
