namespace Vernuntii.Reactive;

/// <summary>
/// Once every <typeparamref name="TWhen"/> event, the <typeparamref name="TEvery"/>
/// event gets subscribed or the existing subscriptions renewed. After new or renewed subscription,
/// every new <typeparamref name="TEvery"/> event gets emitted.
/// </summary>
/// <typeparam name="TWhen"></typeparam>
/// <typeparam name="TEvery"></typeparam>
internal class OnceEveryThenEveryEvent<TWhen, TEvery> : EveryEvent<(TWhen, TEvery)>
{
    private readonly IEmittableEvent<TWhen> _onceEvery;
    private readonly IEmittableEvent<TEvery> _thenEvery;

    public OnceEveryThenEveryEvent(IEmittableEvent<TWhen> onceEvery, IEmittableEvent<TEvery> thenEvery)
    {
        _onceEvery = onceEvery;
        _thenEvery = thenEvery;
    }

    public override IDisposable Subscribe(IEventEmitter<(TWhen, TEvery)> eventEmitter) =>
        DelegatingDisposable.Create(_ => {
            var emptiableDisposables = new DisposableCollection();

            return new DisposableCollection() {
                emptiableDisposables,
                base.Subscribe(eventEmitter),
                _onceEvery.SubscribeUnscheduled(
                (_, onceEvery) => {
                    emptiableDisposables.Dispose(permanently: false);

                    emptiableDisposables.TryAdd(
                        () => _thenEvery.SubscribeUnscheduled((context, thenEvery) => EvaluateEmission(context, (onceEvery, thenEvery))),
                        out var _);
                })
            }.Dispose;
        });
}
