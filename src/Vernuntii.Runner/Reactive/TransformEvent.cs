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

    public IDisposable Subscribe(IEventEmitter<TResult> eventEmitter) =>
        _source.SubscribeUnscheduled(
            static (context, eventData, state) => context.TriggerOrScheduleEventEmission(state.EventEmitter, state.TransformEventData(eventData)),
            (EventEmitter: eventEmitter, TransformEventData: _eventDataTransformHandler));
}
