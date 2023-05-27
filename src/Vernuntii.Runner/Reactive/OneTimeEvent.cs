namespace Vernuntii.Reactive;

internal class OneTimeEvent<T> : EveryEvent<T>
{
    private readonly IEmittableEvent<T> _source;

    public OneTimeEvent(IEmittableEvent<T> source) =>
        _source = source ?? throw new ArgumentNullException(nameof(source));

    public override IDisposable Subscribe(IEventEmitter<T> eventEmitter) =>
        DelegatingDisposable.Create(lifetime => {
            var sourceSubscription = _source.SubscribeUnscheduled((context, eventData) => {
                context.TriggerOrScheduleEventEmission(eventEmitter, eventData);
                //(context with { IsCompleting = true }).TriggerOrScheduleEventEmission(eventEmitter, eventData);
                lifetime.Dispose();
            });

            return sourceSubscription.Dispose;
        });
}
