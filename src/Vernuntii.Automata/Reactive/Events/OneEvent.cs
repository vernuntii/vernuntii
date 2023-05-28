namespace Vernuntii.Reactive.Events;

internal class OneEvent<T> : Event<T>
{
    private readonly IObservableEvent<T> _source;

    public OneEvent(IObservableEvent<T> source) =>
        _source = source ?? throw new ArgumentNullException(nameof(source));

    public override IDisposable Subscribe(IEventObserver<T> eventObserver) =>
        DelegatingDisposable.Create(lifetime => {
            var sourceSubscription = _source.SubscribeBacklogBacked((emissionBacklog, eventData) => {
                eventObserver.OnEmissionOrBacklog(emissionBacklog, eventData);
                lifetime.Dispose();
            });

            return sourceSubscription.Dispose;
        });
}
