namespace Vernuntii.Reactive;

internal class OnceEvent<T> : EveryEvent<T>
{
    private readonly IEmittableEvent<T> _source;

    public OnceEvent(IEmittableEvent<T> source) =>
        _source = source ?? throw new ArgumentNullException(nameof(source));

    public override IDisposable Subscribe(IEventEmitter<T> eventEmitter) =>
        DelegatingDisposable.Create(lifetime => {
            var sourceSubscription = _source.SubscribeUnscheduled((context, eventData) => {
                context.MakeOrScheduleEventEmission(eventEmitter, eventData);
                //(context with { IsCompleting = true }).MakeOrScheduleEventEmission(eventEmitter, eventData);
                lifetime.Dispose();
            });

            return sourceSubscription.Dispose;
        });
}
