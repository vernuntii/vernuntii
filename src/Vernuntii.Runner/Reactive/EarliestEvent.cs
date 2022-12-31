namespace Vernuntii.Reactive;

internal class EarliestEvent<T> : LatestEvent<T>
{
    internal override bool IsCompleted => HasEventData;

    protected override IEventDataHolder<T> CanEvaluate(T eventData)
    {
        if (!HasEventData) {
            return base.CanEvaluate(eventData);
        }

        return this;
    }
}
