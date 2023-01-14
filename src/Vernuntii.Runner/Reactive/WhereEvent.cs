namespace Vernuntii.Reactive;

internal class WhereEvent<TSource, TState> : IEmittableEvent<TSource>
{
    private readonly IEmittableEvent<TSource> _source;
    private readonly Func<TSource, TState, bool> _predicate;
    private readonly TState _predicateState;

    public WhereEvent(IEmittableEvent<TSource> source, Func<TSource, TState, bool> predicate, TState predicateState)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        _predicateState = predicateState;
    }

    public IDisposable Subscribe(IEventEmitter<TSource> eventEmitter) =>
        _source.SubscribeUnscheduled((context, eventData) => {
            if (!_predicate(eventData, _predicateState)) {
                return;
            }

            context.MakeOrScheduleEventEmission(eventEmitter, eventData);
        });
}
