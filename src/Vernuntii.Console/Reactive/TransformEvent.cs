namespace Vernuntii.Reactive;

internal class TransformEvent<TSource, TResult> : IFulfillableEvent<TResult>
{
    private readonly IFulfillableEvent<TSource> _source;
    private readonly Func<TSource, TResult> _eventDataTransformHandler;

    public TransformEvent(IFulfillableEvent<TSource> source, Func<TSource, TResult> transformHandler)
    {
        _source = source;
        _eventDataTransformHandler = transformHandler ?? throw new ArgumentNullException(nameof(transformHandler));
    }

    public IDisposable Subscribe(IEventFulfiller<TResult> eventFilfiller) =>
        DelegatingDisposable.Create(
            _source.SubscribeUnscheduled(
                static (context, eventData, state) => state.EventFilfiller.Fulfill(context, state.TransformEventData(eventData)),
                (EventFilfiller: eventFilfiller, TransformEventData: _eventDataTransformHandler)).Dispose);
}
