namespace Vernuntii.Reactive;

internal static class EventEmissionContextExtensions
{
    public static void ScheduleEventEmission<T>(this EventEmissionContext emissionContext, IEventEmitter<T> eventEmitter, T eventData) =>
        emissionContext.ScheduleEventEmission(
            static result => {
                var (eventHandler, eventData) = (ValueTuple<Func<T, Task>, T>)result;
                return eventHandler(eventData);
            },
            ((Func<T, Task>)eventEmitter.EmitAsync, eventData));

    internal static void TriggerOrScheduleEventEmission<T>(this EventEmissionContext emissionContext, IEventEmitter<T> eventEmitter, T eventData)
    {
        if (eventEmitter.IsEmissionUnschedulable) {
            eventEmitter.Emit(emissionContext, eventData);
            return;
        }

        emissionContext.ScheduleEventEmission(eventEmitter, eventData);
    }
}
