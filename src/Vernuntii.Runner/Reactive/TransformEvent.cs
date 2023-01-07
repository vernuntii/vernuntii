namespace Vernuntii.Reactive;

internal class TransformEvent<TSource, TResult> : IEmittableEvent<TResult>
{
    private readonly IEmittableEvent<TSource> _source;
    private readonly Func<TSource, TResult> _eventDataTransformHandler;

    public TransformEvent(IEmittableEvent<TSource> source, Func<TSource, TResult> transformHandler)
    {
        _source = source;
        _eventDataTransformHandler = transformHandler ?? throw new ArgumentNullException(nameof(transformHandler));
    }

    public IDisposable Subscribe(IEventEmitter<TResult> eventFilfiller) =>
        DelegatingDisposable.Create(
            _source.SubscribeUnscheduled(
                static (context, eventData, state) => state.EventFilfiller.Emit(context, state.TransformEventData(eventData)),
                (EventFilfiller: eventFilfiller, TransformEventData: _eventDataTransformHandler)).Dispose);
}
