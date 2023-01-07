namespace Vernuntii.Reactive;

internal class FirstEvent<T> : EarliestEvent<T>
{
    protected override void MakeEmission(EventEmissionContext context, T eventData) =>
        Emit(context with { IsCompleting = true }, eventData);

    protected override IEventDataHolder<T>? InspectEmission(T eventData)
    {
        if (HasEventData) {
            return null;
        }

        return base.InspectEmission(eventData);
    }
}
