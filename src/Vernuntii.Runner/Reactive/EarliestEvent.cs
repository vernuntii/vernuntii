namespace Vernuntii.Reactive;

/// <summary>
/// On every <typeparamref name="T"/> event the earliest ever appeared event gets emitted.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class EarliestEvent<T> : LatestEvent<T>
{
    protected override IEventDataHolder<T>? InspectEmission(T eventData)
    {
        if (!HasEventData) {
            return base.InspectEmission(eventData);
        }

        return this;
    }
}
