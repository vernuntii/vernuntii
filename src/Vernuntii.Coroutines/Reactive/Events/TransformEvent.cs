namespace Vernuntii.Reactive.Events;

internal class TransformEvent<TSource, TResult> : IObservableEvent<TResult>
{
    private readonly IObservableEvent<TSource> _source;
    private readonly Func<TSource, TResult> _transformer;

    public TransformEvent(IObservableEvent<TSource> source, Func<TSource, TResult> transformer)
    {
        _source = source;
        _transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
    }

    public IDisposable Subscribe(IEventObserver<TResult> eventObserver) =>
        _source.SubscribeBacklogBacked(
            static (emissionBacklog, eventData, state) => state.EventObserver.OnEmissionOrBacklog(emissionBacklog, state.TransformEventData(eventData)),
            (EventObserver: eventObserver, TransformEventData: _transformer));
}
