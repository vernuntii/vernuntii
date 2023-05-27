namespace Vernuntii.Reactive;

/// <summary>
/// Once every <typeparamref name="TWhen"/> event, the <typeparamref name="TThen"/>
/// event gets subscribed or the existing subscriptions renewed. After new or renewed subscription,
/// every new <typeparamref name="TThen"/> event gets emitted.
/// </summary>
/// <typeparam name="TWhen"></typeparam>
/// <typeparam name="TThen"></typeparam>
internal class OnceEveryThenEvent<TWhen, TThen> : EveryEvent<(TWhen, TThen)>
{
    private readonly IEmittableEvent<TWhen> _when;
    private readonly IEmittableEvent<TThen> _then;

    public OnceEveryThenEvent(IEmittableEvent<TWhen> when, IEmittableEvent<TThen> then)
    {
        _when = when;
        _then = then;
    }

    public override IDisposable Subscribe(IEventEmitter<(TWhen, TThen)> eventEmitter) =>
        DelegatingDisposable.Create(_ => {
            var thenDisposables = new DisposableCollection();

            return new DisposableCollection() {
                thenDisposables,
                base.Subscribe(eventEmitter),
                _when.SubscribeUnscheduled(
                (_, when) => {
                    thenDisposables.Dispose(permanently: false);

                    thenDisposables.TryAdd(
                        () => _then.SubscribeUnscheduled((context, then) => EvaluateEmission(context, (when, then))),
                        out var _);
                })
            }.Dispose;
        });
}
